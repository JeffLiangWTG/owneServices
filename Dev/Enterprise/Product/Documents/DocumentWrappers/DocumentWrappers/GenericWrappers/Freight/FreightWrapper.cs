using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.Netting;
using Enterprise.Barcode.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Customs;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.eTail.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Scanning;
using Enterprise.ProcessManagement.Business;
using Enterprise.Rating.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;
using static Enterprise.Integration.Customs.CA;
using static Enterprise.Integration.Customs.US;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperCreator : IDocFreightWrapperCreator
	{
		IDocumentWrapper IDocFreightWrapperCreator.CreateFreightWrapper(BusinessObject parent, BusinessObjectFactory factory)
		{
			return FreightWrapper.NewFreightWrapper(parent, factory);
		}

		Type IDocFreightWrapperCreator.GetFreightWrapperType(Type parentType)
		{
			var factory = new BusinessObjectFactory();
			var bizO = (parentType == typeof(QuotedBooking)) ? QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, factory) : factory.New(parentType);
			var wrapper = FreightWrapper.NewFreightWrapper(bizO, factory);

			return wrapper != null ? wrapper.GetType() : null;
		}
	}

	[DefaultField("JobNumber")]
	[WrapperTypeName("Freight")]
	public class FreightWrapper : GenericWrapperWithNotes, IControlFlightDetailsSuppression, IDocTypeCode, IDocWrapperWithChildBizObjToWrap, IShouldExcludeFromDocPackByDefault
	{
		#region Constructors

		protected FreightWrapper(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			: base(businessObjectToWrap, factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public static FreightWrapper[] New(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			BusinessObject transactionHeader = null;

			if (businessObjectToWrap is AccTransactionHeader && !(businessObjectToWrap is TransactionHeader))
			{
				transactionHeader = factory.Load<TransactionHeader>(businessObjectToWrap.PK);
			}

			var bizObj = transactionHeader ?? businessObjectToWrap;
			var invoice = bizObj as InvoicingBase;
			var cashAdvanceRequest = bizObj as AccCashAdvanceRequestHeader;
			FreightWrapper result = null;

			if (invoice != null)
			{
				bizObj = new InvoicePrintTask.ParentJobLoader(factory).Load(invoice);
				result = NewFreightWrapper(bizObj, factory);

				if (result == null)
				{
					result = NewFreightWrapper(invoice, factory);
				}

				result.arInvoice = DocARInvoice.New(invoice, factory);
				result.genericTransactionHeader = DocGenericTransactionHeader.New(invoice, factory, result);
			}
			else if (cashAdvanceRequest != null)
			{
				bizObj = CashAdvancePrintTask.LoadParentJob(cashAdvanceRequest);
				if (bizObj != null)
				{
					result = NewFreightWrapper(bizObj, factory);
					result.genericTransactionHeader = DocGenericTransactionHeader.New(cashAdvanceRequest, factory, result);
				}
			}
			else if (
				   bizObj is PaymentApprovalBase
				|| bizObj is AccPaymentBatch
				|| bizObj is TransactionHeader
				|| bizObj is PrintStatement
				|| bizObj is PrintSummary
				|| bizObj is AccountingJournal
				|| bizObj is VoucherProvider)
			{
				result = NewFreightWrapper(bizObj, factory);
				result.genericTransactionHeader = DocGenericTransactionHeader.New(bizObj, factory, result);
			}
			else if (bizObj is NettingCentreStatement)
			{
				result = NewFreightWrapper(bizObj, factory);
				result.nettingStatement = DocNettingStatement.New((NettingCentreStatement)bizObj, factory);
			}
			else if (bizObj is ParticipantStatement)
			{
				result = NewFreightWrapper(bizObj, factory);
				result.nettingStatement = DocNettingStatement.New((ParticipantStatement)bizObj, factory);
			}
			else
			{
				result = NewFreightWrapper(bizObj, factory);
			}

			return new FreightWrapper[] { result };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public static FreightWrapper[] New(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
		{
			FreightWrapper[] results = null;
			if (childBizObjToWrap != null)
			{
				var transport = childBizObjToWrap as Transport;
				if (transport != null)
				{
					var shipment = parentBizObjToWrap as ForwardingShipment;
					if (shipment != null)
					{
						results = NewWithForwardingShipment(shipment, transport, factory);
					}
					else
					{
						var declaration = parentBizObjToWrap as BaseJobDeclaration;

						if (declaration != null)
						{
							results = new FreightWrapper[] { FreightWrapperFromDeclaration.New(declaration, transport, factory) };
						}
					}
				}
				else if (childBizObjToWrap is OrgHeader)
				{
					var debtor = (OrgHeader)childBizObjToWrap;
					var shipment = parentBizObjToWrap as ForwardingShipment;

					if (shipment != null)
					{
						results = new FreightWrapper[] { new FreightWrapperFromShipment(shipment, debtor, factory) };
					}
				}
				else if (childBizObjToWrap is HVLVItem hvlvItem && parentBizObjToWrap is HVLVConsignment hvlvConsignment)
				{
					results = new FreightWrapper[] { new FreightWrapperFromHVLVConsignment(hvlvConsignment, hvlvItem, factory) };
				}
				else
				{
					var result = NewFreightWrapper(parentBizObjToWrap, factory);
					if (result != null)
					{
						var wrap = childBizObjToWrap as BaseJobComInvoiceHeader;
						if (wrap != null)
						{
							if (result.CommercialInvoices != null)
							{
								CommercialInvoiceWrapper commercialInvoiceWrapperToKeep = null;

								foreach (CommercialInvoiceWrapper ciWrapper in result.CommercialInvoices)
								{
									if (ciWrapper.WrappedObjectPK == wrap.PK)
									{
										commercialInvoiceWrapperToKeep = ciWrapper;
										break;
									}
								}

								if (commercialInvoiceWrapperToKeep != null)
								{
									result.CommercialInvoices.RemoveAll();
									result.CommercialInvoices.Add(commercialInvoiceWrapperToKeep);
								}
							}

							results = new[] { result };
						}
						else if (childBizObjToWrap is CommonContainer || childBizObjToWrap is BaseCusContainer || childBizObjToWrap is OrderContainer)
						{
							RemoveUnrelatedContainers(result, childBizObjToWrap, true);

							var container = childBizObjToWrap as CommonContainer;
							if (container != null)
							{
								RemoveUnrelatedPackages(result, container.PackLines.ToArray());
							}

							var cartage = parentBizObjToWrap as CommonCartage;
							if (cartage != null && container != null)
							{
								var moves = cartage.GetBookedMoves(container);
								if (moves.Length > 0)
								{
									result.RemoveUnrelatedLocalTransportLegs(moves[0].CartageLegs.ToArray(), false, true);
								}
							}

							results = new[] { result };
						}
						else if (childBizObjToWrap is CommonCartageLeg)
						{
							var cartageLeg = (CommonCartageLeg)childBizObjToWrap;

							result.IsFromCartageLeg = true;
							result.RemoveUnrelatedLocalTransportLegs(new[] { cartageLeg }, true, true);

							if (cartageLeg.Container != null)
							{
								RemoveUnrelatedContainers(result, cartageLeg.Container, false);
								result.Packages.RemoveAll();
							}
							else if (cartageLeg.IsLoose && cartageLeg.BookedCtgMove != null)
							{
								RemoveUnrelatedPackages(result, new[] { cartageLeg.BookedCtgMove });
								result.Containers.RemoveAll();
							}

							results = new[] { result };
						}
						else if (childBizObjToWrap is JobService)
						{
							ServiceWrapperCollection services;

							// if parentBizObjToWrap is a Container, result will be a FreightWrapperFromConsol or similar (parent of the container)
							// therefore remove unrelated Containers AND the Containers Services

							if (parentBizObjToWrap is CommonContainer || parentBizObjToWrap is BaseCusContainer || parentBizObjToWrap is OrderContainer)
							{
								RemoveUnrelatedContainers(result, parentBizObjToWrap, true);
								var containerWrapper = result.Containers[0];
								services = containerWrapper.Services;

								var consolWrapper = result as FreightWrapperFromConsol;
								if (consolWrapper != null)
								{
									consolWrapper.OverriddenSecondaryHeading = Res.GetString("de0ad5db-1484-42d1-8064-dc7a27d61ced", "Container");
									consolWrapper.OverriddenSecondaryJobNumber = containerWrapper.ContainerNumberOrTypeCount;
								}
							}
							else
							{
								services = result.Services;
							}

							ServiceWrapper serviceWrapperToKeep = null;
							foreach (ServiceWrapper serviceWrapper in services)
							{
								if (serviceWrapper.WrappedObjectPK == childBizObjToWrap.PK)
								{
									serviceWrapperToKeep = serviceWrapper;
									break;
								}
							}

							if (serviceWrapperToKeep != null)
							{
								result.Services.RemoveAll();
								result.Services.Add(serviceWrapperToKeep);

								serviceWrapperToKeep.WrapperForReportName = result;

								results = new[] { result };
							}
						}
						else if (childBizObjToWrap is CommonPickupDeliveryConfirm)
						{
							PickupDeliveryConfirmationsWrapper wrapperToNotRemove = null;
							foreach (PickupDeliveryConfirmationsWrapper wrapperToPossiblyKeep in result.PickupDeliveryConfirmations)
							{
								if (wrapperToPossiblyKeep.WrappedObjectPK == childBizObjToWrap.PK)
								{
									wrapperToNotRemove = wrapperToPossiblyKeep;
									break;
								}
							}

							if (wrapperToNotRemove != null)
							{
								if (wrapperToNotRemove.Confirmation.IsContainerised && wrapperToNotRemove.Confirmation.Container != null)
								{
									ContainerWrapper containerWrapperToKeep = null;
									foreach (ContainerWrapper containerWrapper in result.Containers)
									{
										if (containerWrapper.WrappedObjectPK == wrapperToNotRemove.Confirmation.Container.PK)
										{
											containerWrapperToKeep = containerWrapper;
											result.wrappedChildBusinessObject = wrapperToNotRemove.Confirmation.Container;
											break;
										}
									}

									if (containerWrapperToKeep != null)
									{
										result.Containers.RemoveAll();
										result.Containers.Add(containerWrapperToKeep);
									}
									result.Packages.RemoveAll();
								}

								result.PickupDeliveryConfirmations.RemoveAll();
								result.PickupDeliveryConfirmations.Add(wrapperToNotRemove);
							}

							result.ForceCartageInfoDataSource(childBizObjToWrap as CommonPickupDeliveryConfirm, parentBizObjToWrap);

							results = new[] { result };
						}
						else if (childBizObjToWrap is DtbConsignmentConfirmation)
						{
							var consignmentConfirmation = (DtbConsignmentConfirmation)childBizObjToWrap;
							RemoveUnrelatedInstructionsByConfirmation(result, consignmentConfirmation);

							results = new[] { result };
						}
						else if (childBizObjToWrap is PkgPackageHeader)
						{
							var packageJob = parentBizObjToWrap as PkgPackageJob;
							if (packageJob != null)
							{
								((IPackageOverrider)result).SetPackageCollectionOverride(packageHeaders: new[] { (PkgPackageHeader)childBizObjToWrap });
							}
							results = new[] { result };
						}

						var declarationWrapper = result as FreightWrapperFromDeclaration;
						var releaseStatus = childBizObjToWrap as IReleaseStatus;
						if (declarationWrapper != null && releaseStatus != null)
						{
							result.Customs.CA.RemoveUnrelatedReleaseStatuses(releaseStatus);
							results = new[] { result };
						}
					}
				}

				results?.ForEach(r =>
				{
					if (childBizObjToWrap.IsInDatabase && r.wrappedChildBusinessObject == null)
					{
						r.wrappedChildBusinessObject = childBizObjToWrap;
					}
				});
			}

			return results;
		}

		#region VAT numbers for consignor, consignee, notify party
		public ZString ConsigneeRequiredTaxNumber
		{
			get
			{
				if (Consignee != null && Consignee.Organisation != null && !Consignee.Organisation.IsMiscellaneous
					&& Destination != null && Destination.Location.Country != null && !Destination.Location.Country.Code.IsEmpty)
				{
					return GetTaxNumberConsignee(Consignee.Country?.Code, new OrgHeaderRegistrationNumberProvider(Consignee.Organisation));
				}

				return ZString.Empty;
			}
		}

		public ZString ConsignorRequiredTaxNumber
		{
			get
			{
				if (Consignor != null && Consignor.Organisation != null && !Consignor.Organisation.IsMiscellaneous
					&& Origin != null && Origin.Location.Country != null && !Origin.Location.Country.Code.IsEmpty)
				{
					return GetTaxNumber(Consignor.Country?.Code, new OrgHeaderRegistrationNumberProvider(Consignor.Organisation), TaxNumberType.Shipper);
				}

				return ZString.Empty;
			}
		}

		public ZString MasterBillConsigneeOverrideRequiredTaxNumber
		{
			get
			{
				if (MasterBillConsigneeOverride != null && MasterBillConsigneeOverride.Organisation != null && !MasterBillConsigneeOverride.Organisation.IsMiscellaneous
					&& Destination != null && Destination.Location.Country != null && !Destination.Location.Country.Code.IsEmpty)
				{
					return GetTaxNumberConsignee(MasterBillConsigneeOverride.Country?.Code, new OrgHeaderRegistrationNumberProvider(MasterBillConsigneeOverride.Organisation));
				}

				return ZString.Empty;
			}
		}

		public ZString MasterBillShipperOverrideRequiredTaxNumber
		{
			get
			{
				if (MasterBillShipperOverride != null && MasterBillShipperOverride.Organisation != null && !MasterBillShipperOverride.Organisation.IsMiscellaneous
					&& Origin != null && Origin.Location.Country != null && !Origin.Location.Country.Code.IsEmpty)
				{
					return GetTaxNumber(MasterBillShipperOverride.Country?.Code, new OrgHeaderRegistrationNumberProvider(MasterBillShipperOverride.Organisation), TaxNumberType.Shipper);
				}

				return ZString.Empty;
			}
		}

		public ZString NotifyPartyRequiredTaxNumber
		{
			get
			{
				if (NotifyParty != null && NotifyParty.Organisation != null && !NotifyParty.Organisation.IsMiscellaneous
					&& Destination != null && Destination.Location.Country != null && !Destination.Location.Country.Code.IsEmpty)
				{
					return RequiredTaxNumbers.GetRequiredTaxNumberWithType(Destination.Location.Country.Code, string.Empty, NotifyParty.Organisation, RequiredTaxNumbers.TaxOrgType.AlsoNotify, RequiredTaxNumbers.DocumentType.General);
				}

				return ZString.Empty;
			}
		}

		string GetTaxNumberConsignee(string addressCountry, OrgHeaderRegistrationNumberProvider registrationNumberProvider)
		{
			var taxNumber = string.Empty;
			switch (Destination?.Location?.Country?.Code.SubstringSafe(0, 2))
			{
				case Constants.CountryCodes.India:
					{
						taxNumber = "TAX:" + string.Join(", ", GetTaxNumbers(addressCountry, registrationNumberProvider, TaxNumberType.Consignee).Select(x => x.Number.Split(":").LastOrDefault().Trim()).ToArray());
						break;
					}
				default:
					{
						taxNumber = GetTaxNumber(addressCountry, registrationNumberProvider, TaxNumberType.Consignee);
						break;
					}
			}

			return taxNumber;
		}

		ZString GetTaxNumber(string addressCountry, OrgHeaderRegistrationNumberProvider registrationNumberProvider, TaxNumberType taxNumberType)
		{
			var requiredTaxInfos = GetTaxNumbers(addressCountry, registrationNumberProvider, taxNumberType);
			if (requiredTaxInfos != null && requiredTaxInfos.Any())
			{
				var taxNumber = requiredTaxInfos.WhereNotNull().ToList().First().Number;
				if (!string.IsNullOrEmpty(taxNumber))
				{
					return taxNumber;
				}
			}

			return ZString.Empty;
		}

		IEnumerable<TaxInfo> GetTaxNumbers(string addressCountry, OrgHeaderRegistrationNumberProvider registrationNumberProvider, TaxNumberType taxNumberType)
		{
			if (string.IsNullOrEmpty(addressCountry))
			{
				return new List<TaxInfo>();
			}

			var regulatingCountry = taxNumberType == TaxNumberType.Shipper ? Origin?.Location?.Country?.Code : Destination?.Location?.Country?.Code;
			var refDatas = RequiredTaxNumbers.GetTaxInfoFromRefTable(addressCountry ?? ZString.Empty, registrationNumberProvider, Factory, regulatingCountry ?? ZString.Empty);
			var requiredTaxInfos = GetTaxInfoFromRefData(refDatas, regulatingCountry ?? ZString.Empty, taxNumberType).ToList();

			if (Destination.Location.Country.Code.SubstringSafe(0, 2) == Constants.CountryCodes.Egypt)
			{
				var secondaryRegulatingCountry = taxNumberType == TaxNumberType.Shipper ? Destination?.Location?.Country?.Code : Origin?.Location?.Country?.Code;
				var secondaryRefDatas = RequiredTaxNumbers.GetTaxInfoFromRefTable(addressCountry ?? ZString.Empty, registrationNumberProvider, Factory, secondaryRegulatingCountry ?? ZString.Empty);
				var secondaryRequiredTaxInfos = GetTaxInfoFromRefData(secondaryRefDatas, secondaryRegulatingCountry ?? ZString.Empty, taxNumberType).ToList();

				requiredTaxInfos.AddRange(secondaryRequiredTaxInfos);
			}

			return requiredTaxInfos;
		}

		IEnumerable<TaxInfo> GetTaxInfoFromRefData(List<TaxCodeInformation> refTableData, ZString country, TaxNumberType taxNumberType)
		{
			refTableData = refTableData?.Where(t => t.DocumentType == Constants.TaxRelatedDocumentType.HouseBill).ToList();
			if (refTableData == null || !refTableData.Any())
			{
				return Enumerable.Empty<TaxInfo>();
			}

			var taxCodeInformations = refTableData.OrderBy(t => t.Priority).ToList();
			var nonEmptyList = taxCodeInformations.Where(t => t.Number != ZString.Empty).ToList();
			if (nonEmptyList.Any())
			{
				taxCodeInformations = nonEmptyList;
			}
			var usePriority = taxCodeInformations.First().Priority;
			taxCodeInformations = taxCodeInformations.Where(t => t.Priority == usePriority).ToList();

			return taxCodeInformations.Select(t => GenerateTaxInfo(t, country, taxNumberType));
		}

		TaxInfo GenerateTaxInfo(TaxCodeInformation taxCodeInformation, ZString country, TaxNumberType taxNumberType)
		{
			return new TaxInfo
			{
				Code = taxCodeInformation.Code,
				Description = taxCodeInformation.Description,
				ShortLabel = taxCodeInformation.ShortLabel,
				LongLabel = taxCodeInformation.LongLabel,
				Number = GetAndFormatTaxNumber(taxCodeInformation, taxNumberType),
				Country = country,
				IsChinaSpecific = taxCodeInformation.RegulatingCountryCode == Constants.CountryCodes.China,
				RegulatingCountry = taxCodeInformation.RegulatingCountryCode
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		ZString GetAndFormatTaxNumber(TaxCodeInformation taxCodeInformation, TaxNumberType taxNumberType)
		{
			var taxTypeCode = GetTaxTypeCode(taxCodeInformation, taxNumberType);

			if ((Destination?.Location?.Country?.Code.SubstringSafe(0, 2) ?? ZString.Empty) == Constants.CountryCodes.Egypt && taxCodeInformation.RegulatingCountryCode == Constants.CountryCodes.Egypt)
			{
				if (taxNumberType == TaxNumberType.Shipper && !string.IsNullOrEmpty(taxCodeInformation.Number))
				{
					if (string.IsNullOrEmpty(taxCodeInformation.Comments))
					{
						return RequiredTaxNumbers.CombineTaxTypeAndNumber(taxTypeCode, taxCodeInformation.Number);
					}
					var registerType = taxCodeInformation.Comments.EqualsIgnoringCase("Tax Id") ? "02" : "01";
					return RequiredTaxNumbers.CombineTaxTypeAndNumber(taxTypeCode, $"{taxCodeInformation.CountryCode}-{registerType}-{taxCodeInformation.Number}");
				}
			}

			return RequiredTaxNumbers.CombineTaxTypeAndNumber(taxTypeCode, taxCodeInformation.Number);
		}

		ZString GetTaxTypeCode(TaxCodeInformation taxCodeInformation, TaxNumberType taxNumberType)
		{
			if (!taxCodeInformation.ShortLabel.IsEmpty)
			{
				return taxCodeInformation.ShortLabel;
			}

			return taxCodeInformation.Code;
		}

		#endregion

		#region VAT numbers for import and export agents

		public ZString ImportAgentRequiredTaxNumber
		{
			get
			{
				if (ReceivingForwarder != null && ReceivingForwarder.Organisation != null && !ReceivingForwarder.Organisation.IsMiscellaneous
					&& Destination != null && Destination.Location.Country != null && !Destination.Location.Country.Code.IsEmpty)
				{
					return RequiredTaxNumbers.GetRequiredTaxNumberWithType(Destination.Location.Country.Code, string.Empty, ReceivingForwarder.Organisation, RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
				}

				return ZString.Empty;
			}
		}

		public ZString ExportAgentRequiredTaxNumber
		{
			get
			{
				if (SendingForwarder != null && SendingForwarder.Organisation != null && !SendingForwarder.Organisation.IsMiscellaneous
					&& Origin != null && Origin.Location.Country != null && !Origin.Location.Country.Code.IsEmpty)
				{
					if (Destination == null || Destination.Location.Country == null || Destination.Location.Country.Code.IsEmpty)
					{
						return RequiredTaxNumbers.GetRequiredTaxNumberWithType(ZString.Empty, Origin.Location.Country.Code, SendingForwarder.Organisation, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.General);
					}

					return RequiredTaxNumbers.GetRequiredTaxNumberWithType(Destination.Location.Country.Code, Origin.Location.Country.Code, SendingForwarder.Organisation, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.General);
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region RemoveUnrelatedChildren

		protected static void RemoveUnrelatedPackages(FreightWrapper freightWrapper, BusinessObject[] packagesToKeep)
		{
			var packageWrappersToRemove = freightWrapper.Packages.Cast<PackageWrapper>().Where(x => !packagesToKeep.Contains(x.WrappedObject));
			packageWrappersToRemove.ToList().ForEach(x => freightWrapper.Packages.RemoveAndDelete(x));
		}

		static void RemoveUnrelatedContainers(FreightWrapper freightWrapper, BusinessObject containerToKeep, bool forceContainerAsCartageInfo)
		{
			ContainerWrapper containerWrapperToKeep = null;
			foreach (ContainerWrapper containerWrapper in freightWrapper.Containers)
			{
				var customsContainer = containerWrapper.WrappedObject as BaseCusContainer;
				if (customsContainer != null && containerToKeep is CommonContainer)
				{
					if (customsContainer.CO_JC == containerToKeep.PK)
					{
						containerWrapperToKeep = containerWrapper;
						break;
					}
				}
				else if (containerWrapper.WrappedObjectPK == containerToKeep.PK)
				{
					containerWrapperToKeep = containerWrapper;
					break;
				}
			}

			if (containerWrapperToKeep != null)
			{
				freightWrapper.Containers.RemoveAll();
				freightWrapper.Containers.Add(containerWrapperToKeep);

				if (forceContainerAsCartageInfo)
				{
					freightWrapper.ForceCartageInfoDataSourceForContainer(containerToKeep, freightWrapper.WrappedBO);
				}
				KeepUNDGsRelatedToContainer(freightWrapper, containerWrapperToKeep);
			}
		}

		protected virtual void RemoveUnrelatedLocalTransportLegs(CommonCartageLeg[] legsToKeep, bool useFirstLegAsCartageInfo, bool useLegsAsAddressInfo)
		{
			var legWrappersToKeep = new List<LocalTransportLegWrapper>();
			foreach (LocalTransportLegWrapper legWrapper in LocalTransportLegs)
			{
				if (legsToKeep.Contains((CommonCartageLeg)legWrapper.WrappedObject))
				{
					legWrappersToKeep.Add(legWrapper);
				}
			}

			LocalTransportLegs.RemoveAll();
			LocalTransportLegs.AddRange(legWrappersToKeep);

			if (useFirstLegAsCartageInfo && legsToKeep.Length > 0)
			{
				ForceCartageInfoDataSource(legsToKeep[0]);
			}
		}

		protected bool IsFromCartageLeg { get; private set; }

		static void KeepUNDGsRelatedToContainer(FreightWrapper result, ContainerWrapper containerWrapperToKeep)
		{
			var undgsToKeep = new List<UNDGSubstanceWrapper>();
			foreach (UNDGSubstanceWrapper undg in result.UNDGs)
			{
				if (undg.ContainingPackage.ContainerNo == containerWrapperToKeep.ContainerNo)
				{
					undgsToKeep.Add(undg);
				}
			}

			result.UNDGs.RemoveAll();
			result.UNDGs.AddRange(undgsToKeep);
		}

		static void RemoveUnrelatedInstructionsByConfirmation(FreightWrapper freightWrapper, DtbTransportConfirmation confirmation)
		{
			var bookingInstructions = freightWrapper.BookingInstructions;
			var instructionToKeep = bookingInstructions.FirstOrDefault(i => ((InstructionWrapper)i).ActionBO.PK == confirmation.PK);
			bookingInstructions.RemoveAll();
			bookingInstructions.Add(instructionToKeep);
		}

		#endregion

		public delegate FreightWrapper NewFreightWrapperDelegate(BusinessObject businessObjectToWrap, BusinessObjectFactory factory);

		public static readonly Overridable<NewFreightWrapperDelegate> OverridableNewFreightWrapperDelegate = new Overridable<NewFreightWrapperDelegate>();

		public static FreightWrapper NewFreightWrapper(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			var overridden = OverridableNewFreightWrapperDelegate.Value;
			if (overridden != null)
			{
				var wrapper = overridden(businessObjectToWrap, factory);
				if (wrapper != null)
				{
					return wrapper;
				}
			}

			var adviceHeaderToWrap = businessObjectToWrap as DetentionAdviceHeader;
			if (adviceHeaderToWrap != null)
			{
				return new FreightWrapperFromDetentionAdvice(adviceHeaderToWrap, factory);
			}

			var instanceToWrap = businessObjectToWrap as ReleaseInstance;
			if (instanceToWrap != null)
			{
				return new FreightWrapperFromContainerReleaseInstance(instanceToWrap, factory);
			}

			var delarationToWrap = businessObjectToWrap as BaseJobDeclaration;
			if (delarationToWrap != null)
			{
				return FreightWrapperFromDeclaration.New(delarationToWrap, factory);
			}

			var nctsHeaderToWrap = businessObjectToWrap as NctsHeader;
			if (nctsHeaderToWrap != null)
			{
				return FreightWrapperFromNCTS.New(nctsHeaderToWrap, factory);
			}

			var shipmentToWrap = businessObjectToWrap as ForwardingShipment;
			if (shipmentToWrap != null)
			{
				return NewWithForwardingShipment(shipmentToWrap, null, factory)[0];
			}

			var quotedBookingToWrap = businessObjectToWrap as QuotedBooking;
			if (quotedBookingToWrap != null)
			{
				if (quotedBookingToWrap.Booking != null)
				{
					return new FreightWrapperFromQuotedBooking(quotedBookingToWrap, factory);
				}
				else if (quotedBookingToWrap.Quote != null)
				{
					return new FreightWrapperFromOneOffQuote(quotedBookingToWrap.Quote, factory);
				}
			}

			var agencyShipmentToWrap = businessObjectToWrap as AgencyShipment;
			if (agencyShipmentToWrap != null)
			{
				return new FreightWrapperFromAgencyShipment(agencyShipmentToWrap, factory);
			}

			if (businessObjectToWrap is HVLVConsignment hvlvConsignment)
			{
				return new FreightWrapperFromHVLVConsignment(hvlvConsignment, factory);
			}

			if (businessObjectToWrap is HVLVOuterPackage hvlvOuterPackage)
			{
				return new FreightWrapperFromHVLVOuterPackage(hvlvOuterPackage, factory);
			}

			var agencyShipmentContainerToWrap = businessObjectToWrap as AgencyShipmentContainer;
			if (agencyShipmentContainerToWrap != null)
			{
				return new FreightWrapperFromAgencyShipmentContainer(agencyShipmentContainerToWrap, factory);
			}

			var forwardingContainer = businessObjectToWrap as ForwardingContainer;
			if (forwardingContainer != null)
			{
				if (forwardingContainer.Consol != null)
				{
					return new FreightWrapperFromConsol(forwardingContainer.Consol, factory);
				}
				else
				{
					var cusContainer = factory.LoadTop1<BaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, forwardingContainer.PK));
					if (cusContainer != null)
					{
						return FreightWrapperFromDeclaration.New(cusContainer.Declaration, factory);
					}
				}
			}

			var cfsContainer = businessObjectToWrap as CFSContainer;
			if (cfsContainer != null)
			{
				return new FreightWrapperFromCFSContainer(cfsContainer, factory);
			}

			var commonContainer = businessObjectToWrap as CommonContainer;
			if (commonContainer != null)
			{
				var helper = new CartageContainerHelper(factory, commonContainer);
				return new FreightWrapperFromCartage(helper.FirstCartage, factory);
			}

			var jobCartageToWrap = businessObjectToWrap as CommonCartage;
			if (jobCartageToWrap != null)
			{
				return new FreightWrapperFromCartage(jobCartageToWrap, factory);
			}

			var runSheetToWrap = businessObjectToWrap as CommonWorkSheet;
			if (runSheetToWrap != null)
			{
				return new FreightWrapperFromRunSheet(runSheetToWrap, factory);
			}

			var cydReceiveAdvice = businessObjectToWrap as CYDReceiveAdvice;
			if (cydReceiveAdvice != null)
			{
				return new FreightWrapperFromCYDReceiveAdvice(cydReceiveAdvice, factory);
			}

			var cydReleaseAdvice = businessObjectToWrap as CYDReleaseAdvice;
			if (cydReleaseAdvice != null)
			{
				return new FreightWrapperFromCYDReleaseAdvice(cydReleaseAdvice, factory);
			}

			var cydTransportationUnit = businessObjectToWrap as CYDTransportationUnit;
			if (cydTransportationUnit != null)
			{
				return new FreightWrapperFromCYDTransportationUnit(cydTransportationUnit, factory);
			}

			var cydAdHocServiceOrder = businessObjectToWrap as CYDAdHocServiceOrder;
			if (cydAdHocServiceOrder != null)
			{
				return new FreightWrapperFromCYDAdHocServiceOrder(cydAdHocServiceOrder, factory);
			}

			var workOrderHeader = businessObjectToWrap as MNRWorkOrderHeader;
			if (workOrderHeader != null)
			{
				return new FreightWrapperFromMNRWorkOrder(workOrderHeader, factory);
			}

			var consolToWrap = businessObjectToWrap as ForwardingConsol;
			if (consolToWrap != null)
			{
				return new FreightWrapperFromConsol(consolToWrap, factory);
			}

			var gateTransportCYDetail = businessObjectToWrap as GateTransportCYDetail;
			if (gateTransportCYDetail != null)
			{
				return FreightWrapperFromGateTransport.New(gateTransportCYDetail.GateTransport, gateTransportCYDetail, factory);
			}

			var gateTransportCFSDetail = businessObjectToWrap as GateTransportCFSDetail;
			if (gateTransportCFSDetail != null)
			{
				return FreightWrapperFromGateTransport.New(gateTransportCFSDetail.GateTransport, gateTransportCFSDetail, factory);
			}

			var orderToWrap = businessObjectToWrap as Order;
			if (orderToWrap != null)
			{
				return new FreightWrapperFromOrder(orderToWrap, factory);
			}

			var aPOrderToWrap = businessObjectToWrap as AccPayableOrderHeader;
			if (aPOrderToWrap != null)
			{
				return new FreightWrapperFromPayableOrder(aPOrderToWrap, factory);
			}

			var loadListToWrap = businessObjectToWrap as CFSLoadListConsol;
			if (loadListToWrap != null)
			{
				return new FreightWrapperFromCFSLoadList(loadListToWrap, factory);
			}

			var cfsShipmentToWrap = businessObjectToWrap as CFSShipment;
			if (cfsShipmentToWrap != null)
			{
				return new FreightWrapperFromCFSShipment(cfsShipmentToWrap, factory);
			}

			var invoiceToWrap = businessObjectToWrap as InvoicingBase;
			if (invoiceToWrap != null)
			{
				return new FreightWrapperFromInvoice(invoiceToWrap, factory);
			}

			if (businessObjectToWrap is PaymentApprovalBase paymentApprovalToWrap)
			{
				return new FreightWrapperFromPaymentApproval(paymentApprovalToWrap, factory);
			}

			if (businessObjectToWrap is AccPaymentBatch paymentBatch)
			{
				return new FreightWrapperFromPaymentBatch(paymentBatch, factory);
			}

			var detention = businessObjectToWrap as ContainerDetention;
			if (detention != null)
			{
				return new FreightWrapperFromDetentionInvoice(detention, factory);
			}

			var freightWrapperFromWhsBO = GetNewFreightWrapperFromWhsBO(businessObjectToWrap, factory);
			if (freightWrapperFromWhsBO != null)
			{
				return freightWrapperFromWhsBO;
			}

			var receiveConsignment = businessObjectToWrap as WhsItemReceiveConsignment;
			if (receiveConsignment != null)
			{
				return new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, factory);
			}

			var dispatchConsignment = businessObjectToWrap as WhsItemDispatchConsignment;
			if (dispatchConsignment != null)
			{
				return new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment, factory);
			}

			var transitReceiveASN = businessObjectToWrap as WhsItemReceiveASN;
			if (transitReceiveASN != null)
			{
				return new FreightWrapperFromWhsItemReceiveASN(transitReceiveASN, factory);
			}

			var transitDispatchTransportationUnit = businessObjectToWrap as WhsItemDispatchTransportationUnit;
			if (transitDispatchTransportationUnit != null)
			{
				return new FreightWrapperFromWhsItemDispatchTransportationUnit(transitDispatchTransportationUnit, factory);
			}

			var transitReceiveTransportationUnit = businessObjectToWrap as WhsItemReceiveTransportationUnit;
			if (transitReceiveTransportationUnit != null)
			{
				return new FreightWrapperFromWhsItemReceiveTransportationUnit(transitReceiveTransportationUnit, factory);
			}

			var whsLoad = businessObjectToWrap as WhsLoad;
			if (whsLoad != null)
			{
				return new FreightWrapperFromWhsLoad(whsLoad, factory);
			}

			var handlingUnit = businessObjectToWrap as PkgHandlingUnit;
			if (handlingUnit != null)
			{
				return new FreightWrapperFromPkgHandlingUnit(handlingUnit, factory);
			}

			var dispatchLoadList = businessObjectToWrap as WhsItemDispatchLoadList;
			if (dispatchLoadList != null)
			{
				return new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, factory);
			}

			var transferHeader = businessObjectToWrap as WhsItemTransferHeader;
			if (transferHeader != null)
			{
				return new FreightWrapperFromWhsItemTransferHeader(transferHeader, factory);
			}

			var whsInvoiceToWrap = businessObjectToWrap as WhsInvoice;
			if (whsInvoiceToWrap != null)
			{
				return new FreightWrapperFromWhsInvoice(whsInvoiceToWrap, factory);
			}

			var cTOCusHAWBToWrap = businessObjectToWrap as CTOCusHAWB;
			if (cTOCusHAWBToWrap != null)
			{
				return new FreightWrapper(cTOCusHAWBToWrap, factory);
			}

			var cusMAWBToWrap_AU = businessObjectToWrap as Enterprise.Customs.AU.Declaration.Business.CusMAWB;
			if (cusMAWBToWrap_AU != null)
			{
				return new FreightWrapper(cusMAWBToWrap_AU, factory);
			}

			var cusUnderbondToWrap = businessObjectToWrap as Enterprise.Customs.Business.CusUnderbond;
			if (cusUnderbondToWrap != null)
			{
				return new FreightWrapper(cusUnderbondToWrap, factory);
			}

			var jobMawbToWrap = businessObjectToWrap as JobMawb;
			if (jobMawbToWrap != null)
			{
				return new FreightWrapper(jobMawbToWrap, factory);
			}

			var voyageAccountToWrap = businessObjectToWrap as VoyageAccount;
			if (voyageAccountToWrap != null)
			{
				return new FreightWrapperFromVoyageAccount(voyageAccountToWrap, factory);
			}

			var sundryChargesToWrap = businessObjectToWrap as SundryCharges;
			if (sundryChargesToWrap != null)
			{
				return new FreightWrapperFromSundryCharges(sundryChargesToWrap, factory);
			}

			var ratingHeader = businessObjectToWrap as RatingHeader;
			if (ratingHeader != null)
			{
				if (ratingHeader.TH_OneTimeQuote)
				{
					return new FreightWrapperFromOneOffQuote((Quote)ratingHeader, factory);
				}
				else
				{
					return new FreightWrapperFromRatingHeader(ratingHeader, factory);
				}
			}

			var transactionHeaderToWrap = businessObjectToWrap as TransactionHeader;
			if (transactionHeaderToWrap != null)
			{
				return new FreightWrapperFromTransactionHeader(transactionHeaderToWrap, factory);
			}

			var printStatementToWrap = businessObjectToWrap as PrintStatement;
			if (printStatementToWrap != null)
			{
				return new FreightWrapperFromPrintStatement(printStatementToWrap, factory);
			}

			var accountingJournalToWrap = businessObjectToWrap as AccountingJournal;
			if (accountingJournalToWrap != null)
			{
				return new FreightWrapper(accountingJournalToWrap, factory);
			}

			var printSummaryToWrap = businessObjectToWrap as PrintSummary;
			if (printSummaryToWrap != null)
			{
				return new FreightWrapperFromPrintSummary(printSummaryToWrap, factory);
			}

			var bookingConsolidation = businessObjectToWrap as DtbBookingConsolidation;
			if (bookingConsolidation != null)
			{
				return new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, factory);
			}

			var transportBooking = businessObjectToWrap as DtbBooking;
			if (transportBooking != null)
			{
				return new FreightWrapperFromDtbBooking(transportBooking, factory);
			}

			var queryClaim = businessObjectToWrap as AccQueryClaim;
			if (queryClaim != null)
			{
				return new FreightWrapperFromQueryClaim(queryClaim, factory);
			}

			var package = businessObjectToWrap as PkgPackage;
			if (package != null && package.PackageJob != null)
			{
				return new FreightWrapperFromPkgPackageJob(package.PackageJob, factory);
			}

			var packageJob = businessObjectToWrap as PkgPackageJob;
			if (packageJob != null)
			{
				return new FreightWrapperFromPkgPackageJob(packageJob, factory);
			}

			var transportBookingConsignment = businessObjectToWrap as DtbBookingConsignment;
			if (transportBookingConsignment != null)
			{
				return new FreightWrapperFromDtbBookingConsignment(transportBookingConsignment, factory);
			}

			var transportConsignment = businessObjectToWrap as DtbConsignment;
			if (transportConsignment != null)
			{
				return new FreightWrapperFromDtbConsignment(transportConsignment, factory);
			}

			var dtbConsignmentRunSheet = businessObjectToWrap as DtbConsignmentRunSheet;
			if (dtbConsignmentRunSheet != null)
			{
				return new FreightWrapperFromDtbConsignmentRunSheet(dtbConsignmentRunSheet, factory);
			}

			var dtbLinehaulManifest = businessObjectToWrap as DtbLinehaulManifest;
			if (dtbLinehaulManifest != null)
			{
				return new FreightWrapperFromDtbLinehaulManifest(dtbLinehaulManifest, factory);
			}

			var workItem = businessObjectToWrap as WorkItem;
			if (workItem != null)
			{
				return new FreightWrapperFromWorkItem(workItem, factory);
			}

			var workProject = businessObjectToWrap as Project;
			if (workProject != null)
			{
				return new FreightWrapperFromWorkProject(workProject, factory);
			}

			var workRequest = businessObjectToWrap as WorkRequest;
			if (workRequest != null)
			{
				return new FreightWrapperFromCustomerServiceTicket(workRequest, factory);
			}

			var participantStatementToWrap = businessObjectToWrap as NettingStatement;
			if (participantStatementToWrap != null)
			{
				return new FreightWrapper(participantStatementToWrap, factory);
			}

			var area = businessObjectToWrap as WhsArea;
			if (area != null)
			{
				return new WarehouseAreaWrapper(area, factory);
			}

			var organisation = businessObjectToWrap as OrgHeader;
			if (organisation != null)
			{
				return new FreightWrapperFromOrgBO(organisation, factory);
			}

			var opportunity = businessObjectToWrap as OrgOpportunity;
			if (opportunity != null)
			{
				return new FreightWrapperFromOrgOpportunityBO(opportunity, factory);
			}

			var communication = businessObjectToWrap as OrgSalesCall;
			if (communication != null)
			{
				return new FreightWrapperFromOrgSalesCallBO(communication, factory);
			}

			var person = businessObjectToWrap as GlbPerson;
			if (person != null)
			{
				return new FreightWrapperFromGlbPerson(person, factory);
			}

			var accreditationAttempt = businessObjectToWrap as GlbAccreditationAttempt;
			if (accreditationAttempt != null)
			{
				return new FreightWrapperFromAccreditationAttempt(accreditationAttempt, factory);
			}

			var voucherProvider = businessObjectToWrap as VoucherProvider;
			if (voucherProvider != null)
			{
				return new FreightWrapper(voucherProvider, factory);
			}

			if (businessObjectToWrap is JobSupplierBooking jobSupplierBooking)
			{
				return new FreightWrapperFromJobSupplierBooking(jobSupplierBooking, factory);
			}

#if DEBUG
			var dummy = businessObjectToWrap as ZArchitecture.Business.Testing.DummyEnterpriseBusinessObject;
			if (dummy != null)
			{
				return new FreightWrapper(dummy, factory);
			}
#endif

			var isfToWrap = businessObjectToWrap as ICusISFHeader;
			if (isfToWrap != null)
			{
				return (FreightWrapper)DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.US.DocumentWrappers.FreightWrapperFromISF", businessObjectToWrap, "Enterprise.Customs.US.DocumentWrappers");
			}

			var reconDeclarationWrap = businessObjectToWrap as IReconDeclaration;
			if (reconDeclarationWrap != null)
			{
				return (FreightWrapper)DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.US.DocumentWrappers.FreightWrapperFromReconDeclaration", businessObjectToWrap, "Enterprise.Customs.US.DocumentWrappers");
			}

			return null;
		}

		static FreightWrapperFromWhsBO GetNewFreightWrapperFromWhsBO(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			FreightWrapperFromWhsBO freightWrapperFromWhsBO = null;

			if (businessObjectToWrap is WhsAdjustment ||
				businessObjectToWrap is WhsReceive ||
				businessObjectToWrap is WhsOrder ||
				businessObjectToWrap is WhsWorkOrder ||
				businessObjectToWrap is WhsPick ||
				businessObjectToWrap is WhsStocktake ||
				businessObjectToWrap is WhsTransfer)
			{
				freightWrapperFromWhsBO = new FreightWrapperFromWhsBO(businessObjectToWrap, factory);
			}

			return freightWrapperFromWhsBO;
		}

		static FreightWrapper[] NewWithForwardingShipment(ForwardingShipment shipmentToWrap, Transport transportToWrap, BusinessObjectFactory factory)
		{
			var registrySaysUseBrokerageForDocBuilder = DocumentsDataRegistry.Instance.DocBuilderDataSource.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Brokerage;
			if (registrySaysUseBrokerageForDocBuilder && shipmentToWrap.DeclarationForDocuments is BaseJobDeclaration declaration)
			{
				return new FreightWrapper[] { FreightWrapperFromDeclaration.New(declaration, transportToWrap, factory) };
			}
			else
			{
				return new FreightWrapper[] { new FreightWrapperFromShipment(shipmentToWrap, transportToWrap, factory, registrySaysUseBrokerageForDocBuilder) };
			}
		}

		#endregion

		#region Related Business Objects

		protected virtual IFlightDetailsSuppression SuppressingBizO { get; }

		bool IControlFlightDetailsSuppression.ShouldSuppressFlightDetails(SuppressFields fieldType, ContactType contactType)
		{
			return Suppression.EnabledForAnyOfFields(SuppressingBizO, new[] { fieldType }, contactType);
		}

		public BaseJobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = GetDeclaration()); }
		}
		BaseJobDeclaration fDeclaration;
		protected virtual BaseJobDeclaration GetDeclaration() => null;

		public NctsHeader NctsHeader
		{
			get { return fNctsHeader ?? (fNctsHeader = GetNctsHeader()); }
		}
		NctsHeader fNctsHeader;
		protected virtual NctsHeader GetNctsHeader() => null;

		CustomsWrapper customs;
		public CustomsWrapper Customs
		{
			get { return customs ?? (customs = CustomsWrapper.New(new FreightWrapperCustomsInfoAdapter(this), Factory)); }
		}

		public ForwardingShipment FreightShipment
		{
			get { return fShipment ?? (fShipment = GetShipment()); }
		}
		ForwardingShipment fShipment;
		protected virtual ForwardingShipment GetShipment() => null;

		public CFSShipment CFSShipment
		{
			get { return fCFSShipment ?? (fCFSShipment = GetCFSShipment()); }
		}
		CFSShipment fCFSShipment;
		protected virtual CFSShipment GetCFSShipment() => null;

		public QuotedBooking FreightBooking
		{
			get { return quotedBooking ?? (quotedBooking = GetQuotedBooking()); }
		}
		QuotedBooking quotedBooking;
		protected virtual QuotedBooking GetQuotedBooking() => null;

		public HVLVConsignment HVLVConsignment
		{
			get { return hvlvConsignment ?? (hvlvConsignment = GetHVLVConsignment()); }
		}
		HVLVConsignment hvlvConsignment;
		protected virtual HVLVConsignment GetHVLVConsignment() => null;

		public HVLVItem HVLVItem
		{
			get { return hvlvItem ?? (hvlvItem = GetHVLVItem()); }
		}
		HVLVItem hvlvItem;
		protected virtual HVLVItem GetHVLVItem() => null;

		public ForwardingConsol Consol
		{
			get { return fConsol ?? (fConsol = GetConsol()); }
		}
		ForwardingConsol fConsol;
		protected virtual ForwardingConsol GetConsol() => null;

		protected ForwardingConsol GetConsolFromTransport(ForwardingShipment shipment, Transport transport)
		{
			ForwardingConsol consol = null;

			if (transport != null && transport.JW_ParentType == Constants.TransportParentTypes.Consol && transport.Parent != null)
			{
				consol = transport.Parent as ForwardingConsol;
			}
			else if (shipment != null)
			{
				if (shipment.CurrentConsolForDocuments != null)
				{
					consol = shipment.CurrentConsolForDocuments;
				}
				else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
				{
					consol = shipment.ArrivalConsolForDocuments as ForwardingConsol;
				}
				else
				{
					consol = shipment.DepartureConsolForDocuments as ForwardingConsol;
				}
			}

			return consol;
		}

		public CFSLoadListConsol CFSLoadList
		{
			get { return fCFSLoadList ?? (fCFSLoadList = GetCFSLoadList()); }
		}
		CFSLoadListConsol fCFSLoadList;
		protected virtual CFSLoadListConsol GetCFSLoadList() => null;

		public CommonCartage Cartage
		{
			get { return fCartage ?? (fCartage = GetCartage()); }
		}
		CommonCartage fCartage;
		protected virtual CommonCartage GetCartage() => null;

		public Order Order
		{
			get { return fOrder ?? (fOrder = GetOrder()); }
		}
		Order fOrder;
		protected virtual Order GetOrder() => null;

		public AccPayableOrderHeader PayableOrder
		{
			get { return fPayableOrder ?? (fPayableOrder = GetPayableOrder()); }
		}
		AccPayableOrderHeader fPayableOrder;
		protected virtual AccPayableOrderHeader GetPayableOrder() => null;

		public AgencyShipment AgencyShipment
		{
			get { return agencyShipment ?? (agencyShipment = GetAgencyShipment()); }
		}
		AgencyShipment agencyShipment;
		protected virtual AgencyShipment GetAgencyShipment() => null;

		public CommonShipment BaseShipment
		{
			get { return baseShipment ?? (baseShipment = GetBaseShipment()); }
		}
		CommonShipment baseShipment;
		protected virtual CommonShipment GetBaseShipment() => null;

		public WorkItem WorkItem
		{
			get { return workitem ?? (workitem = GetWorkItem()); }
		}
		WorkItem workitem;
		protected virtual WorkItem GetWorkItem() => null;

		public Project Project
		{
			get { return project ?? (project = GetWorkProject()); }
		}
		Project project;
		protected virtual Project GetWorkProject() => null;

		public WorkRequest WorkRequest
		{
			get { return cst ?? (cst = GetWorkRequest()); }
		}
		WorkRequest cst;
		protected virtual WorkRequest GetWorkRequest() => null;

		public INonTransportJobHeaderParent NonTransportBO
		{
			get { return nonTransportBO ?? (nonTransportBO = GetNonTransportBO()); }
		}
		INonTransportJobHeaderParent nonTransportBO;
		protected virtual INonTransportJobHeaderParent GetNonTransportBO() => null;

		public OrganisationWrapper JobHeaderLocalClient
		{
			get
			{
				if (jobHeaderLocalClient == null)
				{
					jobHeaderLocalClient = NewJobHeaderLocalClient() ?? new OrganisationWrapper(OrganisationUsageType.LocalClient, (JobDocAddress)null, Factory);
				}

				return jobHeaderLocalClient;
			}
		}
		protected virtual OrganisationWrapper NewJobHeaderLocalClient()
		{
			return new OrganisationWrapper(OrganisationUsageType.LocalClient, (Job != null ? Job.LocalCharges : null), ContactType.Receivables, Factory);
		}
		OrganisationWrapper jobHeaderLocalClient;

		public Job Job
		{
			get { return job ?? (job = GetJob()); }
		}
		Job job;
		protected virtual Job GetJob() => null;

		public InvoicingJobWrapper InvoicingJob
		{
			get { return invoicingJob ?? (invoicingJob = GetInvoicingJob()); }
		}
		InvoicingJobWrapper invoicingJob;

		protected virtual InvoicingJobWrapper GetInvoicingJob()
		{
			return Job != null ? (Job.LocalCharges != null ? new InvoicingJobWrapper(Job, Job.LocalCharges, Factory) : new InvoicingJobWrapper(Job, Factory)) : null;
		}

		public IReconDeclaration ReconDeclaration
		{
			get { return fReconDeclaration ?? (fReconDeclaration = GetReconDeclaration()); }
		}
		IReconDeclaration fReconDeclaration;

		protected virtual IReconDeclaration GetReconDeclaration() => null;

		public ICusISFHeader ImporterSecurityFiling
		{
			get { return importerSecurityFiling ?? (importerSecurityFiling = GetImporterSecurityFiling()); }
		}
		ICusISFHeader importerSecurityFiling;
		protected virtual ICusISFHeader GetImporterSecurityFiling() => null;

		public SundryCharges SundryCharges
		{
			get { return sundryCharges ?? (sundryCharges = GetSundryCharges()); }
		}
		SundryCharges sundryCharges;
		protected virtual SundryCharges GetSundryCharges() => null;

		#endregion

		#region IZType Properties

		#region Weight Volume Display Options
		protected string WeightVolumeDisplay
		{
			get { return GetWeightVolumeDisplayOption(); }
		}

		protected virtual ZDecimal TotalShipmentWeight
		{
			get { return FreightShipment.GetWeightForDoc(WeightVolumeDisplay); }
		}

		protected virtual ZDecimal TotalShipmentVolume
		{
			get { return FreightShipment.GetVolumeForDoc(WeightVolumeDisplay); }
		}
		#endregion

		public OrganisationWrapper ArrivalCFSTransport
		{
			get { return GetArrivalCFSTransport(); }
		}

		protected virtual OrganisationWrapper GetArrivalCFSTransport()
		{
			return null;
		}

		public OrganisationWrapper DepartureCFSTransport
		{
			get { return GetDepartureCFSTransport(); }
		}

		protected virtual OrganisationWrapper GetDepartureCFSTransport()
		{
			return null;
		}

		public ZString FreightDepotType
		{
			get { return GetFreightDepotType(); }
		}
		protected virtual ZString GetFreightDepotType() => ZString.Empty;

		public ZByte Priority
		{
			get { return GetPriority(); }
		}
		protected virtual ZByte GetPriority() => ZByte.Zero;

		public ZDateTime OrderDate
		{
			get { return GetOrderDate(); }
		}
		protected virtual ZDateTime GetOrderDate() => ZDateTime.Empty;

		public ZDateTime FactoryEx
		{
			get { return GetFactoryEx(); }
		}
		protected virtual ZDateTime GetFactoryEx() => ZDateTime.Empty;

		public ZDateTime ExWorksRequiredBy
		{
			get { return GetExWorksRequiredBy(); }
		}
		protected virtual ZDateTime GetExWorksRequiredBy() => ZDateTime.Empty;

		public ZDateTime ConsolDateCreated
		{
			get { return GetConsolDateCreated(); }
		}
		protected virtual ZDateTime GetConsolDateCreated() => ZDateTime.Empty;

		public ZDateTimeOffset GateInTime
		{
			get { return GetGateInTime(); }
		}
		protected virtual ZDateTimeOffset GetGateInTime() => ZDateTimeOffset.Empty;

		public ZDateTimeOffset GateOutTime
		{
			get { return GetGateOutTime(); }
		}
		protected virtual ZDateTimeOffset GetGateOutTime() => ZDateTimeOffset.Empty;

		/// <summary>
		/// Returns a string of Order References delimited by commas with Owner Ref delimited by "/" but will not return Owner Ref where it is a duplicate of an Order Reference
		/// </summary>
		public ZString OrderNumbersWithOwnersReference
		{
			get { return GetOrderNumbersWithOwnersReference(); }
		}

		protected virtual ZString GetOrderNumbersWithOwnersReference()
		{
			return new ZString((Orders != null ? ((IBODocDataProviderCollection)Orders).Format("{OrderNo}", (NoResString)"Comma", ZString.Empty, ZString.Empty, 0) : ZString.Empty) + " / " + OwnerReference).Trim(' ', '/', ' ');
		}

		public DocBaseJobDeclaration DocDeclaration
		{
			get { return GetDocDeclaration(); }
		}

		protected virtual DocBaseJobDeclaration GetDocDeclaration()
		{
			return null;
		}

		public NctsHeaderDocumentWrapper DocNCTS
		{
			get { return GetDocNCTS(); }
		}

		protected virtual NctsHeaderDocumentWrapper GetDocNCTS()
		{
			return null;
		}

		public ZString ConsolAgentsReference
		{
			get { return GetConsolAgentsReference(); }
		}

		protected virtual ZString GetConsolAgentsReference()
		{
			return ZString.Empty;
		}

		public ZString OrderTrackingNumber
		{
			get { return GetOrderTrackingNumber(); }
		}
		protected virtual ZString GetOrderTrackingNumber() => ZString.Empty;

		public ZString OtherReferences
		{
			get { return GetOtherReferences(); }
		}
		protected virtual ZString GetOtherReferences() => ZString.Empty;

		public ZString ConsolPaymentType
		{
			get { return GetConsolPaymentType(); }
		}
		protected virtual ZString GetConsolPaymentType() => ZString.Empty;

		public ZString BookingReference
		{
			get { return GetBookingReference(); }
		}
		protected virtual ZString GetBookingReference() => ZString.Empty;

		public ZString TransportReferenceHeading
		{
			get { return GetTransportReferenceHeading(); }
		}
		protected virtual ZString GetTransportReferenceHeading()
		{
			return Res.GetString("f2bf074c-bb0c-42e1-9d2e-e13006f79fd9", "REFERENCE");
		}

		public ZString TransportReference
		{
			get { return GetTransportReference(); }
		}
		protected virtual ZString GetTransportReference() => ZString.Empty;

		public CodeAndDescriptionWrapper TransitJobTransportMode
		{
			get { return transitJobTransportMode ?? (transitJobTransportMode = GetTransitJobTransportMode()); }
		}
		CodeAndDescriptionWrapper transitJobTransportMode;
		protected virtual CodeAndDescriptionWrapper GetTransitJobTransportMode() => CodeAndDescriptionWrapper.Empty;

		public ZString CustomerReference
		{
			get { return GetCustomerReference(); }
		}
		protected virtual ZString GetCustomerReference() => ZString.Empty;

		public OrgCarrierAccountWrapper CarrierAccount
		{
			get { return GetOrgCarrierAccount(); }
		}
		protected virtual OrgCarrierAccountWrapper GetOrgCarrierAccount()
		{
			return null;
		}

		public ZString TransportZone
		{
			get { return GetTransportZoneCore(); }
		}
		protected virtual ZString GetTransportZoneCore()
		{
			return ZString.Empty;
		}

		public ZBool Refrigerated
		{
			get { return GetRefrigerated(); }
		}
		protected virtual ZBool GetRefrigerated() => ZBool.False;

		public ZBool Hazardous
		{
			get { return GetHazardous(); }
		}
		protected virtual ZBool GetHazardous() => ZBool.False;
		public ZString MasterBill
		{
			get { return GetMasterBill(); }
		}
		protected virtual ZString GetMasterBill() => ZString.Empty;

		public ZDateTime MasterBillIssue
		{
			get { return GetMasterBillIssue(); }
		}
		protected virtual ZDateTime GetMasterBillIssue() => ZDateTime.Empty;

		public ZString ConsolNumber
		{
			get { return GetConsolNumber(); }
		}
		protected virtual ZString GetConsolNumber() => ZString.Empty;

		public ZString JobNumberHeading
		{
			get { return GetJobNumberHeading(); }
		}
		protected virtual ZString GetJobNumberHeading() => ZString.Empty;

		public ZString JobNumber
		{
			get { return GetJobNumber(); }
		}
		protected virtual ZString GetJobNumber() => ZString.Empty;

		protected virtual ZString TextForBarcode
		{
			get { return DocManagerUniqueID; }
		}

		protected sealed override ZString DocManagerUniqueID
		{
			get { return JobNumber; }
		}

		public ZString SecondaryHeading
		{
			get { return GetSecondaryHeading(); }
		}
		protected virtual ZString GetSecondaryHeading() => ZString.Empty;

		public ZString SecondaryNumber
		{
			get { return GetSecondaryNumber(); }
		}
		protected virtual ZString GetSecondaryNumber() => ZString.Empty;

		public ZDateTime ShipmentDateCreated
		{
			get { return GetShipmentDateCreated(); }
		}
		protected virtual ZDateTime GetShipmentDateCreated() => ZDateTime.Empty;

		public ZString CustomAttribute1
		{
			get { return GetCustomAttribute1(); }
		}
		protected virtual ZString GetCustomAttribute1() => ZString.Empty;

		public ZString CustomAttribute2
		{
			get { return GetCustomAttribute2(); }
		}
		protected virtual ZString GetCustomAttribute2() => ZString.Empty;

		public ZDateTime CustomDate1
		{
			get { return GetCustomDate1(); }
		}
		protected virtual ZDateTime GetCustomDate1() => ZDateTime.Empty;

		public ZDateTime CustomDate2
		{
			get { return GetCustomDate2(); }
		}
		protected virtual ZDateTime GetCustomDate2() => ZDateTime.Empty;

		public ZDecimal CustomDecimal1
		{
			get { return GetCustomDecimal1(); }
		}
		protected virtual ZDecimal GetCustomDecimal1() => ZDecimal.Zero;

		public ZDecimal CustomDecimal2
		{
			get { return GetCustomDecimal2(); }
		}
		protected virtual ZDecimal GetCustomDecimal2() => ZDecimal.Zero;

		public ZBool CustomFlag1
		{
			get { return GetCustomFlag1(); }
		}
		protected virtual ZBool GetCustomFlag1() => ZBool.False;

		public ZBool CustomFlag2
		{
			get { return GetCustomFlag2(); }
		}
		protected virtual ZBool GetCustomFlag2() => ZBool.False;

		public ZString LocalForwarderReference
		{
			get { return GetLocalForwarderReference(); }
		}
		protected virtual ZString GetLocalForwarderReference() => ZString.Empty;

		public ZString ExportAgentsReference
		{
			get { return GetExportAgentsReference(); }
		}
		protected virtual ZString GetExportAgentsReference() => ZString.Empty;

		public ZString ImportAgentsReference
		{
			get { return GetImportAgentsReference(); }
		}
		protected virtual ZString GetImportAgentsReference() => ZString.Empty;

		public ZString ConsolReference
		{
			get { return GetConsolReference(); }
		}
		protected virtual ZString GetConsolReference() => ZString.Empty;

		public ZString GoodsDescription
		{
			get { return GetGoodsDescription(); }
		}
		protected virtual ZString GetGoodsDescription() => ZString.Empty;

		public ZString MarksAndNumbers
		{
			get { return GetMarksAndNumbers(); }
		}
		protected virtual ZString GetMarksAndNumbers() => ZString.Empty;

		public ZString ConNote
		{
			get { return GetConNote(); }
		}
		protected virtual ZString GetConNote() => ZString.Empty;

		public ZString OwnerReference
		{
			get { return GetOwnerReference(); }
		}
		protected virtual ZString GetOwnerReference() => ZString.Empty;

		public ZString HouseBill
		{
			get { return GetHouseBill(); }
		}
		protected virtual ZString GetHouseBill() => ZString.Empty;

		public ZDateTime HouseBillIssue
		{
			get { return GetHouseBillIssue(); }
		}
		protected virtual ZDateTime GetHouseBillIssue() => ZDateTime.Empty;

		public ZString HBLContainerMode
		{
			get { return GetHBLContainerMode(); }
		}
		protected virtual ZString GetHBLContainerMode() => ZString.Empty;

		public ZString ContainerSummary
		{
			get { return Containers != null ? Containers.ContainerSummary : ZString.Empty; }
		}

		public ZString ContainerCount
		{
			get { return Containers != null ? (ZString)Containers.ContainerCount.ToString() : ZString.Empty; }
		}

		#region VendorID

		public ZString VendorID
		{
			get { return GetVendorID(); }
		}
		protected virtual ZString GetVendorID() => ZString.Empty;

		#endregion

		#region Container Layout Type

		public enum ContainerLayoutStyleName
		{
			NoContainers,
			SingleContainer,
			MultipleContainersSingleDetails,
			MultipleContainersMultipleDetails,
		}

		public ZString ContainerLayoutStyle
		{
			get
			{
				var result = ContainerLayoutStyleName.NoContainers;

				if (Containers != null)
				{
					if (Containers.Count == 1)
					{
						result = ContainerLayoutStyleName.SingleContainer;
					}
					else if (Containers.Count > 1)
					{
						ZInt releaseNumCount = 0;
						ZInt emptyReqByCount = 0;
						ZInt pickupDateCount = 0;

						foreach (ContainerWrapper container in Containers)
						{
							if (!container.ReleaseNumber.IsEmpty)
							{
								releaseNumCount++;
							}

							if (!container.EmptyRequired.IsEmpty)
							{
								emptyReqByCount++;
							}

							if (!container.DepartureEstimatedPickup.IsEmpty)
							{
								pickupDateCount++;
							}
						}

						if (releaseNumCount <= 1 && emptyReqByCount <= 1 && pickupDateCount <= 1)
						{
							result = ContainerLayoutStyleName.MultipleContainersSingleDetails;
						}
						else
						{
							result = ContainerLayoutStyleName.MultipleContainersMultipleDetails;
						}
					}
				}

				return result.ToString();
			}
		}

		#endregion

		public Image TACImage
		{
			get
			{
				if (tacImage != null && tacImage.IsDisposed())
				{
					tacImage = null;
				}
				return tacImage ?? (tacImage = GetTACImage());
			}
		}
		Image tacImage;
		protected virtual Image GetTACImage() => null;

		public ZBool HasTACImage
		{
			get { return TACImage != null; }
		}

		public ZDateTime ShippedOnBoardDate
		{
			get { return GetShippedOnBoardDate(); }
		}
		protected virtual ZDateTime GetShippedOnBoardDate() => ZDateTime.Empty;

		public ZInt NoOriginalBills
		{
			get { return GetNoOriginalBills(); }
		}
		protected virtual ZInt GetNoOriginalBills() => ZInt.Zero;

		public ZInt NoCopyBills
		{
			get { return GetNoCopyBills(); }
		}
		protected virtual ZInt GetNoCopyBills() => ZInt.Zero;

		public ZBool IsDomestic
		{
			get { return ShipmentType != null && ShipmentType.Code == "DOM"; }
		}

		public ZDateTimeOffset RevisedDeliveryDueDate
		{
			get { return GetRevisedDeliveryDueDate(); }
		}
		protected virtual ZDateTimeOffset GetRevisedDeliveryDueDate() => ZDateTimeOffset.Empty;

		public ZDateTime DeliveryDueDate
		{
			get { return GetDeliveryDueDate(); }
		}
		protected virtual ZDateTime GetDeliveryDueDate() => ZDateTime.Empty;

		public ZDateTime DeliveryFrom
		{
			get { return GetDeliveryFrom(); }
		}
		protected virtual ZDateTime GetDeliveryFrom() => ZDateTime.Empty;

		public ZDateTime DeliveryRequiredBy
		{
			get { return GetDeliveryRequiredBy(); }
		}
		protected virtual ZDateTime GetDeliveryRequiredBy() => ZDateTime.Empty;

		public ZDateTime DeliveryActual
		{
			get { return GetDeliveryActual(); }
		}
		protected virtual ZDateTime GetDeliveryActual()
		{
			return ZDateTime.Empty;
		}

		public ZDateTime DeliveryCartageAdvised
		{
			get { return GetDeliveryCartageAdvised(); }
		}
		protected virtual ZDateTime GetDeliveryCartageAdvised() => ZDateTime.Empty;

		public ZDateTime DeliveryGoodsDelivered
		{
			get { return GetDeliveryGoodsDelivered(); }
		}
		protected virtual ZDateTime GetDeliveryGoodsDelivered() => ZDateTime.Empty;

		public ZDateTime DeliveryEstimated
		{
			get { return GetDeliveryEstimated(); }
		}
		protected virtual ZDateTime GetDeliveryEstimated()
		{
			return DeliveryFrom;
		}

		public ZDateTime PickupFrom
		{
			get { return GetPickupFrom(); }
		}
		protected virtual ZDateTime GetPickupFrom() => ZDateTime.Empty;

		public ZDateTime PickupRequiredBy
		{
			get { return GetPickupRequiredBy(); }
		}
		protected virtual ZDateTime GetPickupRequiredBy() => ZDateTime.Empty;

		public ZString PackagesDetails
		{
			get { return GetPackagesDetails(); }
		}
		protected virtual ZString GetPackagesDetails()
		{
			return ZString.Empty;
		}

		public ZDateTime PickupActual
		{
			get { return GetPickupActual(); }
		}
		protected virtual ZDateTime GetPickupActual()
		{
			return ZDateTime.Empty;
		}

		public ZDateTime PickupCartageAdvised
		{
			get { return GetPickupCartageAdvised(); }
		}
		protected virtual ZDateTime GetPickupCartageAdvised() => ZDateTime.Empty;

		public ZDateTime PickupGoodsPickedup
		{
			get { return GetPickupGoodsPickedup(); }
		}
		protected virtual ZDateTime GetPickupGoodsPickedup() => ZDateTime.Empty;

		public ZDateTime PickupDateOfReceipt
		{
			get { return GetPickupDateOfReceipt(); }
		}
		protected virtual ZDateTime GetPickupDateOfReceipt() => ZDateTime.Empty;

		public ZString PickupInterimReceipt
		{
			get { return GetPickupInterimReceipt(); }
		}
		protected virtual ZString GetPickupInterimReceipt() => ZString.Empty;

		public ZString WarehouseLocation
		{
			get { return GetWarehouseLocation(); }
		}
		protected virtual ZString GetWarehouseLocation() => ZString.Empty;

		public ZDateTime ActualReceive
		{
			get { return GetActualReceive(); }
		}
		protected virtual ZDateTime GetActualReceive() => ZDateTime.Empty;

		public ZDecimal PickupLabourCharge
		{
			get { return GetPickupLabourCharge(); }
		}
		protected virtual ZDecimal GetPickupLabourCharge() => ZDecimal.Zero;

		public ZString PickupLabourTime
		{
			get { return GetPickupLabourTime(); }
		}
		protected virtual ZString GetPickupLabourTime() => ZString.Empty;

		public ZDecimal PickupTruckWaitCharge
		{
			get { return GetPickupTruckWaitCharge(); }
		}
		protected virtual ZDecimal GetPickupTruckWaitCharge() => ZDecimal.Zero;

		public ZString PickupTruckWaitTime
		{
			get { return GetPickupTruckWaitTime(); }
		}
		protected virtual ZString GetPickupTruckWaitTime() => ZString.Empty;

		public ZString ShippersReference
		{
			get { return GetShippersReference(); }
		}
		protected virtual ZString GetShippersReference() => ZString.Empty;

		public ZString ArrivalReference
		{
			get { return GetArrivalReference(); }
		}
		protected virtual ZString GetArrivalReference() => ZString.Empty;

		public ZDecimal UnAllocatedWeight
		{
			get
			{
				ZDecimal result = 0m;
				if (ThereAreContainersToPackThisShipmentIn && ShipmentCanBeContainerised)
				{
					ZDecimal totalShipmentWeightPackedInContainers = 0m;
					var unitToCalculateIn = BaseShipment.Containers.FirstOrDefault()?.JC_Calc_TotalWeightUnit ?? BaseShipment.ShipmentWeightUnit;
					foreach (var container in BaseShipment.Containers)
					{
						totalShipmentWeightPackedInContainers += Constants.Weight.Convert(container.JC_Calc_TotalWeight, container.JC_Calc_TotalWeightUnit, unitToCalculateIn, false);
					}

					var convertedTotalShipmentWeight = Constants.Weight.Convert(TotalShipmentWeight, BaseShipment.ShipmentWeightUnit, unitToCalculateIn, false);
					if (convertedTotalShipmentWeight > totalShipmentWeightPackedInContainers)
					{
						result = convertedTotalShipmentWeight - totalShipmentWeightPackedInContainers;
					}
				}
				return result;
			}
		}

		public ZDecimal UnAllocatedVolume
		{
			get
			{
				ZDecimal result = 0m;
				if (ThereAreContainersToPackThisShipmentIn && ShipmentCanBeContainerised)
				{
					ZDecimal totalShipmentVolumePackedInContainers = 0m;
					var unitToCalculateIn = BaseShipment.Containers.FirstOrDefault()?.JC_Calc_TotalVolumeUnit ?? BaseShipment.ShipmentVolumeUnit;
					foreach (var container in BaseShipment.Containers)
					{
						totalShipmentVolumePackedInContainers += Constants.Volume.Convert(container.JC_Calc_TotalVolume, container.JC_Calc_TotalVolumeUnit, unitToCalculateIn, false);
					}

					var convertedTotalShipmentVolume = Constants.Volume.Convert(TotalShipmentVolume, BaseShipment.ShipmentVolumeUnit, unitToCalculateIn, false);
					if (convertedTotalShipmentVolume > totalShipmentVolumePackedInContainers)
					{
						result = convertedTotalShipmentVolume - totalShipmentVolumePackedInContainers;
					}
				}
				return result;
			}
		}

		public ZInt UnAllocatedPackages
		{
			get
			{
				ZInt result = 0;
				if (ThereAreContainersToPackThisShipmentIn && ShipmentCanBeContainerised)
				{
					ZInt totalShipmentPackagesInContainers = 0;
					foreach (var container in BaseShipment.Containers)
					{
						totalShipmentPackagesInContainers += container.JC_Calc_TotalPackages;
					}

					if (BaseShipment.JS_OuterPacks > totalShipmentPackagesInContainers)
					{
						result = BaseShipment.JS_OuterPacks - totalShipmentPackagesInContainers;
					}
				}
				return result;
			}
		}

		protected virtual bool ThereAreContainersToPackThisShipmentIn
		{
			get { return (Consol != null && Consol.Containers.Count != 0 && FreightShipment != null); }
		}

		protected virtual bool ShipmentCanBeContainerised
		{
			get
			{
				return FreightShipment.JS_PackingMode != Constants.ContainerModes.BreakBulk
					&& FreightShipment.JS_PackingMode != Constants.ContainerModes.Bulk
					&& FreightShipment.JS_PackingMode != Constants.ContainerModes.Liquid;
			}
		}

		public ZString CTOArrivalBerth
		{
			get { return GetCTOArrivalBerth(); }
		}
		protected virtual ZString GetCTOArrivalBerth() => ZString.Empty;

		public ZString MasterBillHeading
		{
			get { return GetMasterBillHeading(); }
		}
		protected virtual ZString GetMasterBillHeading() => ZString.Empty;

		public ZString HouseBillHeading
		{
			get { return GetHouseBillHeading(); }
		}
		protected virtual ZString GetHouseBillHeading() => ZString.Empty;

		public ZString AdditionalTerms
		{
			get { return GetAdditionalTerms(); }
		}
		protected virtual ZString GetAdditionalTerms() => ZString.Empty;

		public ZString CargoControlNumberForCanada
		{
			get { return GetCargoControlNumberForCanada(); }
		}
		protected virtual ZString GetCargoControlNumberForCanada() => GetReferenceNumberByTypeAndCountry(CanadaAdditionalReferenceNumberTypes.Codes.CCN, Constants.CountryCodes.Canada);

		public ZString PreviousCargoControlNumberForCanada
		{
			get { return GetPreviousCargoControlNumberForCanada(); }
		}
		protected virtual ZString GetPreviousCargoControlNumberForCanada() => GetReferenceNumberByTypeAndCountry(CanadaAdditionalReferenceNumberTypes.Codes.PCN, Constants.CountryCodes.Canada);

		protected ZString GetReferenceNumberByTypeAndCountry(ZString referenceNumberType, ZString countryCode)
		{
			var result = ZString.Empty;

			CusEntryNumber cusEntryNumber = null;
			if (FreightShipment != null)
			{
				cusEntryNumber = FreightShipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(referenceNumberType, countryCode);
			}

			if (cusEntryNumber == null && Consol != null)
			{
				cusEntryNumber = Consol.Numbers.GetFirstReferenceNumberByTypeAndCountry(referenceNumberType, countryCode);
			}

			if (cusEntryNumber != null)
			{
				result = GetFormattedReferenceNumber(cusEntryNumber.CE_EntryNum);
			}
			return result;
		}

		static ZString GetFormattedReferenceNumber(ZString unformattedCargoControlNumber)
		{
			var result = unformattedCargoControlNumber;
			const int checkposition = 4;

			if (unformattedCargoControlNumber.Length > checkposition &&
				unformattedCargoControlNumber.IndexOfAny(new char[] { ' ', '-' }, checkposition) == checkposition)
			{
				result = unformattedCargoControlNumber.Remove(checkposition, 1);
			}
			return result;
		}

		public ZDecimal LoadingMeters
		{
			get { return GetLoadingMeters(); }
		}
		protected virtual ZDecimal GetLoadingMeters() => ZDecimal.Zero;

		public ZString QuoteNumber
		{
			get { return GetQuoteNumber(); }
		}
		protected virtual ZString GetQuoteNumber() => ZString.Empty;

		public ZString HouseACIDNo
		{
			get { return GetHouseACIDNo(); }
		}
		protected virtual ZString GetHouseACIDNo() => ZString.Empty;

		#region Area Fields

		#region WarehouseName

		public ZString WarehouseName
		{
			get { return GetWarehouseName(); }
		}

		protected virtual ZString GetWarehouseName()
		{
			return ZString.Empty;
		}

		#endregion

		#region AreaName

		public ZString AreaName
		{
			get { return GetAreaName(); }
		}

		protected virtual ZString GetAreaName()
		{
			return ZString.Empty;
		}

		#endregion

		#region AreaBarcode

		public ZString AreaBarcode
		{
			get { return GetAreaBarcode(); }
		}

		protected virtual ZString GetAreaBarcode()
		{
			return ZString.Empty;
		}

		#endregion

		#region IsAuthorisedToLeave

		public ZBool IsAuthorisedToLeave => GetIsAuthorisedToLeave();

		protected virtual ZBool GetIsAuthorisedToLeave() => ZBool.False;

		#endregion

		#endregion

		#endregion

		#region Wrapper Properties

		#region FormattedTotalCO2e

		public ZString FormattedTotalCO2e
		{
			get { return formattedTotalCO2e ?? (formattedTotalCO2e = GetFormattedTotalCO2e()).Value; }
		}
		ZString? formattedTotalCO2e;

		protected virtual ZString GetFormattedTotalCO2e() => ZString.Empty;

		#endregion

		#region CO2eCalculationDate

		public ZDateTime CO2eCalculationDate
		{
			get { return cO2eCalculationDate ?? (cO2eCalculationDate = GetCO2eCalculationDate()).Value; }
		}
		ZDateTime? cO2eCalculationDate;

		protected virtual ZDateTime GetCO2eCalculationDate() => ZDateTime.Empty;

		#endregion

		#region FullHandlingInstructions

		public ZString FullHandlingInstructions
		{
			get { return fullHandlingInstructions ?? (fullHandlingInstructions = GetFullHandlingInstructions()).Value; }
		}
		ZString? fullHandlingInstructions;

		protected virtual ZString GetFullHandlingInstructions() => ZString.Empty;

		#endregion

		#region FullCartageInstructions

		public ZString FullCartageInstructions
		{
			get { return fullCartageInstructions ?? (fullCartageInstructions = GetFullCartageInstructions()).Value; }
		}

		ZString? _fullCartageInstructions;

		public ZString? fullCartageInstructions
		{
			get { return _fullCartageInstructions; }
			set { _fullCartageInstructions = value; }
		}

		protected virtual ZString GetFullCartageInstructions() => ZString.Empty;

		public ZString AWBSecurityInspectionStatus
		{
			get { return GetAWBSecurityInspectionStatus(); }
		}

		protected virtual ZString GetAWBSecurityInspectionStatus() => ZString.Empty;

		#endregion

		public StaffWrapper SalesRep
		{
			get { return salesRep ?? (salesRep = GetSalesRep()); }
		}
		protected virtual StaffWrapper GetSalesRep()
		{
			return (Job != null && Job.RepSales != null) ? new StaffWrapper(Job.RepSales, Factory) : null;
		}
		StaffWrapper salesRep;

		#region Person

		public PersonWrapper Person
		{
			get { return person ?? (person = GetPersonWrapper()); }
		}
		PersonWrapper person;
		protected virtual PersonWrapper GetPersonWrapper() => null;

		public AccreditationAttemptWrapper AccreditationAttempt
		{
			get { return accreditationAttemptWrapper ?? (accreditationAttemptWrapper = GetAccreditationAttemptWrapper()); }
		}
		AccreditationAttemptWrapper accreditationAttemptWrapper;
		protected virtual AccreditationAttemptWrapper GetAccreditationAttemptWrapper() => null;

		#endregion

		#region Job Supplierr Booking

		public DocJobSupplierBooking JobSupplierBooking => jobSupplierBookingWrapper ?? (jobSupplierBookingWrapper = GetJobSupplierBookingWrapper());
		DocJobSupplierBooking jobSupplierBookingWrapper;

		protected virtual DocJobSupplierBooking GetJobSupplierBookingWrapper() => null;

		#endregion

		public RatingWrapper Rating
		{
			get { return rating ?? (rating = GetRating()); }
		}
		RatingWrapper rating;
		protected virtual RatingWrapper GetRating() => null;

		public CodeAndDescriptionWrapper ConsolType
		{
			get { return fConsolType ?? (fConsolType = GetConsolType()); }
		}
		CodeAndDescriptionWrapper fConsolType;
		protected virtual CodeAndDescriptionWrapper GetConsolType() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper ConsolContainerMode
		{
			get { return fConsolContainerMode ?? (fConsolContainerMode = GetConsolContainerMode()); }
		}
		CodeAndDescriptionWrapper fConsolContainerMode;
		protected virtual CodeAndDescriptionWrapper GetConsolContainerMode() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper OrderTransportMode
		{
			get { return fOrderTransportMode ?? (fOrderTransportMode = GetOrderTransportMode()); }
		}
		CodeAndDescriptionWrapper fOrderTransportMode;
		protected virtual CodeAndDescriptionWrapper GetOrderTransportMode() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper ConsolTransportMode
		{
			get { return fConsolTransportMode ?? (fConsolTransportMode = GetConsolTransportMode()); }
		}
		CodeAndDescriptionWrapper fConsolTransportMode;
		protected virtual CodeAndDescriptionWrapper GetConsolTransportMode() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper InspectionType
		{
			get { return fInspectionType ?? (fInspectionType = GetInspectionType()); }
		}
		CodeAndDescriptionWrapper fInspectionType;
		protected virtual CodeAndDescriptionWrapper GetInspectionType() => CodeAndDescriptionWrapper.Empty;

		public CartageInfoWrapper CartageInfo
		{
			get { return cartageInfo ?? (cartageInfo = GetCartageInfo()); }
		}
		CartageInfoWrapper cartageInfo;
		protected virtual CartageInfoWrapper GetCartageInfo() => CartageInfoWrapper.New(Factory);

		void ForceCartageInfoDataSource(CommonCartageLeg cartageLeg)
		{
			cartageInfo = CartageInfoWrapper.New(DocCommonCartageLeg.New(cartageLeg, Factory), Factory);
		}

		void ForceCartageInfoDataSource(CommonPickupDeliveryConfirm confirm, BusinessObject parentBizObjToWrap)
		{
			cartageInfo = CartageInfoWrapper.New(DocPickupDeliveryConfirm.New(confirm, (CommonShipment)parentBizObjToWrap, Factory), Factory);
		}

		void ForceCartageInfoDataSourceForContainer(BusinessObject childBizObjToWrap, BusinessObject parentBizObjToWrap)
		{
			var container = childBizObjToWrap as CommonContainer;
			var cusContainer = childBizObjToWrap as BaseCusContainer;

			if (container != null)
			{
				var cartage = parentBizObjToWrap as CommonCartage;
				if (cartage != null)
				{
					cartageInfo = CartageInfoWrapper.New(DocCommonContainer.New(container, cartage, Factory), Factory);
				}
				else
				{
					var declaration = parentBizObjToWrap as BaseJobDeclaration;
					if (declaration != null)
					{
						cartageInfo = CartageInfoWrapper.New(container, declaration.Shipment, Factory);
					}
					else
					{
						cartageInfo = CartageInfoWrapper.New(container, parentBizObjToWrap as CommonShipment, Factory);
					}
				}
			}
			else if (cusContainer != null)
			{
				var declaration = cusContainer.Declaration;
				if (declaration != null)
				{
					var customsContainerWrapper = DocumentWrapperFactory.CreateCustomsContainerWrapperWithDeclaration(cusContainer, declaration, declaration.CountryCode) as DocBaseCusContainer;
					cartageInfo = CartageInfoWrapper.New(customsContainerWrapper, Factory);
				}
			}
		}

		public OrganisationWrapper Carrier
		{
			get { return fCarrier ?? (fCarrier = GetCarrier()); }
		}
		OrganisationWrapper fCarrier;
		protected virtual OrganisationWrapper GetCarrier() => new OrganisationWrapper(OrganisationUsageType.Carrier, Factory.GetNull<JobDocAddress>(), Factory);

		#region CarrierIsNMFCEnabled

		public ZBool CarrierIsNMFCEnabled
		{
			get
			{
				return
					Carrier != null &&
					Carrier.Organisation.CustomsCodes.Cast<OrgCusCode>().Any(c => c.IsNMFCParticipant);
			}
		}

		#endregion

		public AddressWrapper ExportReceivingDepotAddress
		{
			get { return exportReceivingDepotAddress ?? (exportReceivingDepotAddress = GetExportReceivingDepotAddress()); }
		}
		AddressWrapper exportReceivingDepotAddress;
		protected virtual AddressWrapper GetExportReceivingDepotAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper ImportArrivalCTOAddress
		{
			get { return importArrivalCTOAddress ?? (importArrivalCTOAddress = GetImportArrivalCTOAddress()); }
		}
		AddressWrapper importArrivalCTOAddress;
		protected virtual AddressWrapper GetImportArrivalCTOAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper ExportReceivingCTOAddress
		{
			get { return exportReceivingCTOAddress ?? (exportReceivingCTOAddress = GetExportReceivingCTOAddress()); }
		}
		AddressWrapper exportReceivingCTOAddress;
		protected virtual AddressWrapper GetExportReceivingCTOAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper ExportReceivalAddress
		{
			get { return exportReceivalAddress ?? (exportReceivalAddress = GetExportReceivalAddress()); }
		}
		AddressWrapper exportReceivalAddress;
		protected virtual AddressWrapper GetExportReceivalAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper ContainerYardEmptyPickupAddress
		{
			get { return fContainerYardEmptyPickupAddress ?? (fContainerYardEmptyPickupAddress = GetContainerYardEmptyPickupAddress()); }
		}
		AddressWrapper fContainerYardEmptyPickupAddress;

		protected virtual AddressWrapper GetContainerYardEmptyPickupAddress()
		{
			return new AddressWrapper(null, ContactType.All, Factory);
		}

		public AddressWrapper ContainerYardEmptyReturnAddress
		{
			get { return fContainerYardEmptyReturnAddress ?? (fContainerYardEmptyReturnAddress = GetContainerYardEmptyReturnAddress()); }
		}
		AddressWrapper fContainerYardEmptyReturnAddress;

		protected virtual AddressWrapper GetContainerYardEmptyReturnAddress()
		{
			return new AddressWrapper(null, ContactType.All, Factory);
		}

		public OrganisationWrapper ConsolCreditor
		{
			get { return fConsolCreditor ?? (fConsolCreditor = GetConsolCreditor()); }
		}
		OrganisationWrapper fConsolCreditor;
		protected virtual OrganisationWrapper GetConsolCreditor() => new OrganisationWrapper(OrganisationUsageType.ConsolCreditor, Factory.GetNull<JobDocAddress>(), Factory);

		#region ScanningBarcodes

		public CodeAndDesriptionWrapperCollection ScanningBarcodes
		{
			get { return scanningBarcodes ?? (scanningBarcodes = GetScanningBarcodes()); }
		}

		CodeAndDesriptionWrapperCollection GetScanningBarcodes()
		{
			var list = new CodeDescriptionPairList();
			var barcodes = new CodeAndDesriptionWrapperCollection(Factory);

			foreach (var barcode in PackingBarcodes.Barcodes)
			{
				var barcodeText = new TextBarcode(barcode).TextAs128sFontString;
				list.AddPair(barcodeText, barcode);

				var wrapper = new CodeAndDescriptionWrapper(barcodeText, list, Factory);
				barcodes.Add(wrapper);
			}

			return barcodes;
		}

		CodeAndDesriptionWrapperCollection scanningBarcodes;

		#endregion

		public CodeAndDescriptionWrapper ShipmentType
		{
			get { return fShipmentType ?? (fShipmentType = GetShipmentType()); }
		}
		CodeAndDescriptionWrapper fShipmentType;
		protected virtual CodeAndDescriptionWrapper GetShipmentType() => CodeAndDescriptionWrapper.Empty;

		public CodeDescriptionPairList Shipment_Type_List
		{
			get
			{
				var shipmentTypeList = new CodeDescriptionPairList();
				shipmentTypeList.AddPair("IMP", Res.GetString("32ea5b1d-9e8f-4205-8281-fa8e0bf2d2a7", "Import"));
				shipmentTypeList.AddPair("EXP", Res.GetString("cbe20dab-3cb5-4903-996a-ac4df8fc7301", "Export"));
				shipmentTypeList.AddPair("DOM", Res.GetString("fc06569b-742e-4025-b79c-cfb5cde4a682", "Domestic"));
				return shipmentTypeList;
			}
		}

		public CodeAndDescriptionWrapper ShipmentStatus
		{
			get { return fShipmentStatus ?? (fShipmentStatus = GetShipmentStatus()); }
		}
		CodeAndDescriptionWrapper fShipmentStatus;
		protected virtual CodeAndDescriptionWrapper GetShipmentStatus() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper ShipmentContainerMode
		{
			get { return fShipmentContainerMode ?? (fShipmentContainerMode = GetShipmentContainerMode()); }
		}
		CodeAndDescriptionWrapper fShipmentContainerMode;
		protected virtual CodeAndDescriptionWrapper GetShipmentContainerMode() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper ShipmentTransportMode
		{
			get { return fShipmentTransportMode ?? (fShipmentTransportMode = GetShipmentTransportMode()); }
		}
		CodeAndDescriptionWrapper fShipmentTransportMode;
		protected virtual CodeAndDescriptionWrapper GetShipmentTransportMode() => CodeAndDescriptionWrapper.Empty;

		public ZString ApprovalNumber
		{
			get { return GetApprovalNumber(); }
		}
		protected virtual ZString GetApprovalNumber() => ZString.Empty;

		public SupplierBuyerLinkWrapper SupplierBuyerLink
		{
			get { return fSupplierBuyerLink ?? (fSupplierBuyerLink = GetSupplierBuyerLink()); }
		}
		SupplierBuyerLinkWrapper fSupplierBuyerLink;
		protected virtual SupplierBuyerLinkWrapper GetSupplierBuyerLink() => null;

		#region Organization Wrappers

		public OrganisationWrapper Consignor
		{
			get { return fConsignor ?? (fConsignor = GetConsignor()); }
		}
		OrganisationWrapper fConsignor;
		protected virtual OrganisationWrapper GetConsignor() => new OrganisationWrapper(OrganisationUsageType.Consignor, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper Consignee
		{
			get { return fConsignee ?? (fConsignee = GetConsignee()); }
		}
		OrganisationWrapper fConsignee;
		protected virtual OrganisationWrapper GetConsignee() => new OrganisationWrapper(OrganisationUsageType.Consignee, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper Buyer
		{
			get { return fBuyer ?? (fBuyer = GetBuyer()); }
		}
		OrganisationWrapper fBuyer;
		protected virtual OrganisationWrapper GetBuyer() => new OrganisationWrapper(OrganisationUsageType.Buyer, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper RecommendedAgent
		{
			get { return fRecommendedAgent ?? (fRecommendedAgent = GetRecommendedAgent()); }
		}
		OrganisationWrapper fRecommendedAgent;
		protected virtual OrganisationWrapper GetRecommendedAgent() => null;

		public OrganisationWrapper Supplier
		{
			get { return fSupplier ?? (fSupplier = GetSupplier()); }
		}
		OrganisationWrapper fSupplier;
		protected virtual OrganisationWrapper GetSupplier() => new OrganisationWrapper(OrganisationUsageType.Supplier, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper InsuredBy
		{
			get { return fInsuredBy ?? (fInsuredBy = GetInsuredBy()); }
		}
		OrganisationWrapper fInsuredBy;
		protected virtual OrganisationWrapper GetInsuredBy() => new OrganisationWrapper(OrganisationUsageType.InsuredBy, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper AssuredParty
		{
			get { return fAssuredParty ?? (fAssuredParty = GetAssuredParty()); }
		}
		OrganisationWrapper fAssuredParty;
		protected virtual OrganisationWrapper GetAssuredParty() => new OrganisationWrapper(OrganisationUsageType.AssuredParty, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper ClaimsPayableBy
		{
			get { return fClaimsPayableBy ?? (fClaimsPayableBy = GetClaimsPayableBy()); }
		}
		OrganisationWrapper fClaimsPayableBy;
		protected virtual OrganisationWrapper GetClaimsPayableBy() => new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper SurveyReportParty
		{
			get { return fSurveyReportParty ?? (fSurveyReportParty = GetSurveyReportParty()); }
		}
		OrganisationWrapper fSurveyReportParty;
		protected virtual OrganisationWrapper GetSurveyReportParty() => new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper Principal
		{
			get { return principal ?? (principal = GetPrincipal()); }
		}
		OrganisationWrapper principal;
		protected virtual OrganisationWrapper GetPrincipal() => new OrganisationWrapper(OrganisationUsageType.Principal, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper MainShipToParty
		{
			get { return fMainShipToParty ?? (fMainShipToParty = GetMainShipToParty()); }
		}
		OrganisationWrapper fMainShipToParty;
		protected virtual OrganisationWrapper GetMainShipToParty() => new OrganisationWrapper(OrganisationUsageType.MainShipToParty, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper SellingParty
		{
			get { return fSellingParty ?? (fSellingParty = GetSellingParty()); }
		}
		OrganisationWrapper fSellingParty;
		protected virtual OrganisationWrapper GetSellingParty() => new OrganisationWrapper(OrganisationUsageType.SellingParty, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper Consolidator
		{
			get { return fConsolidator ?? (fConsolidator = GetConsolidator()); }
		}
		OrganisationWrapper fConsolidator;
		protected virtual OrganisationWrapper GetConsolidator() => new OrganisationWrapper(OrganisationUsageType.Consolidator, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper StuffingLocation
		{
			get { return fStuffingLocation ?? (fStuffingLocation = GetStuffingLocation()); }
		}
		OrganisationWrapper fStuffingLocation;
		protected virtual OrganisationWrapper GetStuffingLocation() => new OrganisationWrapper(OrganisationUsageType.StuffingLocation, Factory.GetNull<JobDocAddress>(), Factory);

		public LocalForwarderOrganisationWrapper LocalForwarder
		{
			get { return fLocalForwarder ?? (fLocalForwarder = GetLocalForwarder()); }
		}
		LocalForwarderOrganisationWrapper fLocalForwarder;
		protected virtual LocalForwarderOrganisationWrapper GetLocalForwarder() => new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, Factory.GetNull<JobDocAddress>(), Factory);

		public ExportAgentOrganisationWrapper ExportAgent
		{
			get { return fExportAgent ?? (fExportAgent = GetExportAgent()); }
		}
		ExportAgentOrganisationWrapper fExportAgent;
		protected virtual ExportAgentOrganisationWrapper GetExportAgent() => new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper ExportBroker
		{
			get { return fExportBroker ?? (fExportBroker = GetExportBroker()); }
		}
		OrganisationWrapper fExportBroker;
		protected virtual OrganisationWrapper GetExportBroker() => new OrganisationWrapper(OrganisationUsageType.ExportBroker, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper ImportAgent
		{
			get { return fImportAgent ?? (fImportAgent = GetImportAgent()); }
		}
		OrganisationWrapper fImportAgent;
		protected virtual OrganisationWrapper GetImportAgent() => new OrganisationWrapper(OrganisationUsageType.ImportAgent, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper ImportBroker
		{
			get { return fImportBroker ?? (fImportBroker = GetImportBroker()); }
		}
		OrganisationWrapper fImportBroker;
		protected virtual OrganisationWrapper GetImportBroker() => new OrganisationWrapper(OrganisationUsageType.ImportBroker, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper NotifyParty
		{
			get { return fNotifyParty ?? (fNotifyParty = GetNotifyParty()); }
		}
		OrganisationWrapper fNotifyParty;
		protected virtual OrganisationWrapper GetNotifyParty() => new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper NotifyParty2
		{
			get { return notifyParty2 ?? (notifyParty2 = GetNotifyParty2()); }
		}
		OrganisationWrapper notifyParty2;

		protected virtual OrganisationWrapper GetNotifyParty2()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper NotifyParty3
		{
			get { return notifyParty3 ?? (notifyParty3 = GetNotifyParty3()); }
		}
		OrganisationWrapper notifyParty3;

		protected virtual OrganisationWrapper GetNotifyParty3()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper BookingParty
		{
			get { return bookingParty ?? (bookingParty = GetBookingParty()); }
		}
		OrganisationWrapper bookingParty;
		protected virtual OrganisationWrapper GetBookingParty() => new OrganisationWrapper(OrganisationUsageType.BookingParty, Factory.GetNull<JobDocAddress>(), Factory);

		#region Receiving Forwarder

		public OrganisationWrapper ReceivingForwarder
		{
			get { return this.receivingForwarder ?? (this.receivingForwarder = GetReceivingForwarder()); }
		}
		OrganisationWrapper receivingForwarder;

		protected virtual OrganisationWrapper GetReceivingForwarder() => new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, Factory.GetNull<JobDocAddress>(), Factory);

		#endregion

		#region Sending Forwarder

		public ExportAgentOrganisationWrapper SendingForwarder
		{
			get { return this.sendingForwarder ?? (this.sendingForwarder = GetSendingForwarder()); }
		}
		ExportAgentOrganisationWrapper sendingForwarder;

		protected virtual ExportAgentOrganisationWrapper GetSendingForwarder() => new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, Factory.GetNull<JobDocAddress>(), Factory);

		#endregion

		public OrganisationWrapper Client
		{
			get { return client ?? (client = GetClient()); }
		}
		OrganisationWrapper client;
		protected virtual OrganisationWrapper GetClient() => new OrganisationWrapper(OrganisationUsageType.Client, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper ControllingAgent
		{
			get { return controllingAgent ?? (controllingAgent = GetControllingAgent()); }
		}
		OrganisationWrapper controllingAgent;

		protected virtual OrganisationWrapper GetControllingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.ControllingAgent, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper ControllingCustomer
		{
			get { return controllingCustomer ?? (controllingCustomer = GetControllingCustomer()); }
		}
		OrganisationWrapper controllingCustomer;

		protected virtual OrganisationWrapper GetControllingCustomer()
		{
			return new OrganisationWrapper(OrganisationUsageType.ControllingCustomer, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper CarrierBookingAgent
		{
			get { return carrierBookingAgent ?? (carrierBookingAgent = GetCarrierBookingAgent()); }
		}
		OrganisationWrapper carrierBookingAgent;

		protected virtual OrganisationWrapper GetCarrierBookingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.CarrierBookingAgent, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper CarrierHandlingAgent
		{
			get { return carrierHandlingAgent ?? (carrierHandlingAgent = GetCarrierHandlingAgent()); }
		}
		OrganisationWrapper carrierHandlingAgent;

		protected virtual OrganisationWrapper GetCarrierHandlingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.CarrierHandlingAgent, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper MasterBillShipperOverride
		{
			get { return masterBillShipperOverride ?? (masterBillShipperOverride = GetMasterBillShipperOverride()); }
		}
		OrganisationWrapper masterBillShipperOverride;

		protected virtual OrganisationWrapper GetMasterBillShipperOverride()
		{
			return new OrganisationWrapper(OrganisationUsageType.MasterBillShipperOverride, Factory.GetNull<JobDocAddress>(), Factory);
		}

		public OrganisationWrapper MasterBillConsigneeOverride
		{
			get { return masterBillConsigneeOverride ?? (masterBillConsigneeOverride = GetMasterBillConsigneeOverride()); }
		}
		OrganisationWrapper masterBillConsigneeOverride;

		protected virtual OrganisationWrapper GetMasterBillConsigneeOverride()
		{
			return new OrganisationWrapper(OrganisationUsageType.MasterBillConsigneeOverride, Factory.GetNull<JobDocAddress>(), Factory);
		}

		DocOrganisation orgDocument;
		public DocOrganisation OrgDocument
		{
			get
			{
				return orgDocument ?? (orgDocument = GetOrgDocument());
			}
		}

		protected virtual DocOrganisation GetOrgDocument()
		{
			return null;
		}

		DocOrgOpportunity orgOpportunityDocument;
		public DocOrgOpportunity OrgOpportunityDocument
		{
			get
			{
				return orgOpportunityDocument ?? (orgOpportunityDocument = GetOrgOpportunityDocument());
			}
		}

		protected virtual DocOrgOpportunity GetOrgOpportunityDocument()
		{
			return null;
		}

		DocSalesCall orgSalesCallDocument;
		public DocSalesCall OrgSalesCallDocument
		{
			get
			{
				return orgSalesCallDocument ?? (orgSalesCallDocument = GetOrgSalesCallDocument());
			}
		}

		protected virtual DocSalesCall GetOrgSalesCallDocument()
		{
			return null;
		}

		SalesRelationsWrapper salesRelations;
		public SalesRelationsWrapper SalesRelations
		{
			get
			{
				return salesRelations ?? (salesRelations = GetSalesRelationsWrapper());
			}
		}

		protected virtual SalesRelationsWrapper GetSalesRelationsWrapper()
		{
			return null;
		}

		DocOrgSupplierBuyerLink orgSupplierLink;
		public DocOrgSupplierBuyerLink OrgSupplierLink
		{
			get
			{
				return orgSupplierLink ?? (orgSupplierLink = GetOrgSupplierLink());
			}
		}

		protected virtual DocOrgSupplierBuyerLink GetOrgSupplierLink()
		{
			return null;
		}

		DocOrgSupplierBuyerLink orgBuyerLink;
		public DocOrgSupplierBuyerLink OrgBuyerLink
		{
			get
			{
				return orgBuyerLink ?? (orgBuyerLink = GetOrgBuyerLink());
			}
		}

		protected virtual DocOrgSupplierBuyerLink GetOrgBuyerLink()
		{
			return null;
		}

		#endregion

		public PlaceAndDateWrapper Origin
		{
			get { return fOrigin ?? (fOrigin = GetOrigin()); }
		}
		PlaceAndDateWrapper fOrigin;
		protected virtual PlaceAndDateWrapper GetOrigin() => new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);

		public PlaceAndDateWrapper Destination
		{
			get { return fDestination ?? (fDestination = GetDestination()); }
		}
		PlaceAndDateWrapper fDestination;
		protected virtual PlaceAndDateWrapper GetDestination() => new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);

		public PlaceAndDateWrapper FirstForeignPort
		{
			get { return fFirstForeignPort ?? (fFirstForeignPort = GetFirstForeignPort()); }
		}
		PlaceAndDateWrapper fFirstForeignPort;

		protected virtual PlaceAndDateWrapper GetFirstForeignPort()
		{
			return new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		public PlaceAndDateWrapper LastForeignPort
		{
			get { return fLastForeignPort ?? (fLastForeignPort = GetLastForeignPort()); }
		}
		PlaceAndDateWrapper fLastForeignPort;

		protected virtual PlaceAndDateWrapper GetLastForeignPort()
		{
			return new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		public PlaceAndDateWrapper PortOfFirstArrival
		{
			get { return fPortOfFirstArrival ?? (fPortOfFirstArrival = GetPortOfFirstArrival()); }
		}
		PlaceAndDateWrapper fPortOfFirstArrival;

		protected virtual PlaceAndDateWrapper GetPortOfFirstArrival()
		{
			return new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		public LocationWrapper PickupLocation
		{
			get { return fPickupLocation ?? (fPickupLocation = GetPickupLocation()); }
		}
		LocationWrapper fPickupLocation;

		protected virtual LocationWrapper GetPickupLocation()
		{
			if (Origin != null)
			{
				return Origin.Location;
			}
			else
			{
				return new LocationWrapper(null, Factory);
			}
		}

		public LocationWrapper DeliveryLocation
		{
			get { return fDeliveryLocation ?? (fDeliveryLocation = GetDeliveryLocation()); }
		}
		LocationWrapper fDeliveryLocation;

		protected virtual LocationWrapper GetDeliveryLocation()
		{
			if (Destination != null)
			{
				return Destination.Location;
			}
			else
			{
				return new LocationWrapper(null, Factory);
			}
		}

		public LocationWrapper FreightPayableAt
		{
			get { return fFreightPayableAt ?? (fFreightPayableAt = GetFreightPayableAt()); }
		}
		LocationWrapper fFreightPayableAt;

		protected virtual LocationWrapper GetFreightPayableAt()
		{
			return new LocationWrapper(null, Factory);
		}

		public PackQTYWrapper ShipmentInnerPacksQty
		{
			get { return fShipmentInnerPacksQty ?? (fShipmentInnerPacksQty = GetShipmentInnerPacksQty()); }
		}
		PackQTYWrapper fShipmentInnerPacksQty;
		protected virtual PackQTYWrapper GetShipmentInnerPacksQty() => new PackQTYWrapper(ZInt.Zero, ZString.Empty, new CodeDescriptionPairList(), Factory);

		public ValueAndUnitWrapper StorageTime
		{
			get { return fStorageTime ?? (fStorageTime = GetStorageTime()); }
		}
		ValueAndUnitWrapper fStorageTime;
		protected virtual ValueAndUnitWrapper GetStorageTime() => ValueAndUnitWrapper.Empty;

		public PackQTYWrapper ShipmentOuterPacksQty
		{
			get { return fShipmentOuterPacksQty ?? (fShipmentOuterPacksQty = GetShipmentOuterPacksQty()); }
		}
		PackQTYWrapper fShipmentOuterPacksQty;
		protected virtual PackQTYWrapper GetShipmentOuterPacksQty() => new PackQTYWrapper(ZInt.Zero, ZString.Empty, new CodeDescriptionPairList(), Factory);

		public WeightWrapper Weight
		{
			get { return fWeight ?? (fWeight = GetWeight()); }
		}
		WeightWrapper fWeight;
		protected virtual WeightWrapper GetWeight() => WeightWrapper.Empty;

		public VolumeWrapper Volume
		{
			get { return fVolume ?? (fVolume = GetVolume()); }
		}
		VolumeWrapper fVolume;
		protected virtual VolumeWrapper GetVolume() => VolumeWrapper.Empty;

		public MoneyWrapper GoodsValue
		{
			get { return fGoodsValue ?? (fGoodsValue = GetGoodsValue()); }
		}
		MoneyWrapper fGoodsValue;
		protected virtual MoneyWrapper GetGoodsValue() => new MoneyWrapper(new Money(ZDecimal.Zero, Factory.GetNull<RefCurrency>()), Factory);

		public MoneyWrapper InsuranceValue
		{
			get { return insuranceValue ?? (insuranceValue = GetInsuranceValue()); }
		}
		MoneyWrapper insuranceValue;
		protected virtual MoneyWrapper GetInsuranceValue() => new MoneyWrapper(new Money(ZDecimal.Zero, Factory.GetNull<RefCurrency>()), Factory);

		public ValueAndUnitWrapper ChargeableWeight
		{
			get { return fChargeableWeight ?? (fChargeableWeight = GetChargeableWeight()); }
		}
		ValueAndUnitWrapper fChargeableWeight;
		protected virtual ValueAndUnitWrapper GetChargeableWeight() => new ValueAndUnitWrapper(ZInt.Zero, ZString.Empty, new CodeDescriptionPairList(), Factory);

		public MoneyWrapper FreightRate
		{
			get { return fFreightRate ?? (fFreightRate = GetFreightRate()); }
		}
		MoneyWrapper fFreightRate;
		protected virtual MoneyWrapper GetFreightRate() => new MoneyWrapper(new Money(ZDecimal.Zero, Factory.GetNull<RefCurrency>()), Factory);

		[CodeStringFinderHint(typeof(RefServiceLevel), "get_RS_DescriptionMultilingual")]
		public CodeAndDescriptionWrapper ServiceLevel
		{
			get { return fServiceLevel ?? (fServiceLevel = GetServiceLevel()); }
		}
		CodeAndDescriptionWrapper fServiceLevel;
		protected virtual CodeAndDescriptionWrapper GetServiceLevel() => CodeAndDescriptionWrapper.Empty;

		public CarrierServiceLevelWrapper CarrierServiceLevel
		{
			get { return fCarrierServiceLevel ?? (fCarrierServiceLevel = GetCarrierServiceLevel()); }
		}
		CarrierServiceLevelWrapper fCarrierServiceLevel;
		protected virtual CarrierServiceLevelWrapper GetCarrierServiceLevel() => CarrierServiceLevelWrapper.Empty;

		public IncoTermWrapper IncoTerm
		{
			get
			{
				if (fIncoTerm == null && GetIncoTerm() is IncoTermWrapper incoterm)
				{
					fIncoTerm = incotermCodeMappings.TryGetValue(incoterm.Code, out var mapping)
					? new IncoTermWrapper(mapping.Code, mapping.Description, incoterm)
					: incoterm;
				}

				return fIncoTerm;
			}
		}
		IncoTermWrapper fIncoTerm;
		protected virtual IncoTermWrapper GetIncoTerm() => IncoTermWrapper.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Incoterm short-descriptions not translated")]
		const string FreeCarrierDescription = "Free Carrier";
		readonly Dictionary<string, (string Code, string Description)> incotermCodeMappings = new Dictionary<string, (string, string)>
		{
			{ Constants.IncoTerms.FreeCarrier, (Constants.IncoTerms.FreeCarrier, FreeCarrierDescription) },
			{ Constants.IncoTerms.FreeCarrierSeller, (Constants.IncoTerms.FreeCarrier, FreeCarrierDescription) },
			{ Constants.IncoTerms.FreeCarrierBuyer, (Constants.IncoTerms.FreeCarrier, FreeCarrierDescription) }
		};

		public MoneyWrapper CollectAmount
		{
			get { return fGetCollectAmount ?? (fGetCollectAmount = GetCollectAmount()); }
		}
		MoneyWrapper fGetCollectAmount;

		protected virtual MoneyWrapper GetCollectAmount()
		{
			return new MoneyWrapper(Money.Empty, Factory);
		}

		public CodeAndDescriptionWrapper ReleaseType
		{
			get { return fReleaseType ?? (fReleaseType = GetReleaseType()); }
		}
		CodeAndDescriptionWrapper fReleaseType;
		protected virtual CodeAndDescriptionWrapper GetReleaseType() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper ShippedOnBoardType
		{
			get { return fShippedOnBoardType ?? (fShippedOnBoardType = GetShippedOnBoardType()); }
		}
		CodeAndDescriptionWrapper fShippedOnBoardType;
		protected virtual CodeAndDescriptionWrapper GetShippedOnBoardType() => CodeAndDescriptionWrapper.Empty;

		public OrganisationWrapper DeliveryAgent
		{
			get { return fDeliveryAgent ?? (fDeliveryAgent = GetDeliveryAgent()); }
		}
		OrganisationWrapper fDeliveryAgent;
		protected virtual OrganisationWrapper GetDeliveryAgent() => new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper DeliveryAddress
		{
			get { return fDeliveryAddress ?? (fDeliveryAddress = GetDeliveryAddress()); }
		}
		AddressWrapper fDeliveryAddress;
		protected virtual AddressWrapper GetDeliveryAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper PickupAgent
		{
			get { return fPickupAgent ?? (fPickupAgent = GetPickupAgent()); }
		}
		OrganisationWrapper fPickupAgent;
		protected virtual OrganisationWrapper GetPickupAgent() => new OrganisationWrapper(OrganisationUsageType.PickupAgent, Factory.GetNull<JobDocAddress>(), Factory);

		public OrganisationWrapper CTOArrival
		{
			get { return fCTOArrival ?? (fCTOArrival = GetCTOArrival()); }
		}
		OrganisationWrapper fCTOArrival;
		protected virtual OrganisationWrapper GetCTOArrival() => new OrganisationWrapper(OrganisationUsageType.CTOArrival, Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper PickupAddress
		{
			get { return fPickupAddress ?? (fPickupAddress = GetPickupAddress()); }
		}
		AddressWrapper fPickupAddress;
		protected virtual AddressWrapper GetPickupAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public CodeAndDescriptionWrapper CaratagePickupMode
		{
			get { return fCaratagePickupMode ?? (fCaratagePickupMode = GetCaratagePickupMode()); }
		}
		CodeAndDescriptionWrapper fCaratagePickupMode;
		protected virtual CodeAndDescriptionWrapper GetCaratagePickupMode() => CodeAndDescriptionWrapper.Empty;

		public AddressWrapper PickupCFSAddress
		{
			get { return fPickupCFSAddress ?? (fPickupCFSAddress = GetPickupCFSAddress()); }
		}
		AddressWrapper fPickupCFSAddress;
		protected virtual AddressWrapper GetPickupCFSAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper UnpackCFSAddress
		{
			get { return fUnpackCFSAddress ?? (fUnpackCFSAddress = GetUnpackCFSAddress()); }
		}
		AddressWrapper fUnpackCFSAddress;
		protected virtual AddressWrapper GetUnpackCFSAddress() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public AddressWrapper GoodsAvailableAt
		{
			get { return fGoodsAvailableAt ?? (fGoodsAvailableAt = GetGoodsAvailableAt()); }
		}
		AddressWrapper fGoodsAvailableAt;
		protected virtual AddressWrapper GetGoodsAvailableAt() => new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);

		public RouteWrapper InterestedRoute
		{
			get { return fInterestedRoute ?? (fInterestedRoute = GetInterestedRoute()); }
		}
		RouteWrapper fInterestedRoute;
		protected virtual RouteWrapper GetInterestedRoute() => new RouteWrapper(Factory.GetNull<Transport>(), Factory);

		public ContainerWrapperCollection TranshipmentContainers
		{
			get { return GetTranshipmentContainers(); }
		}
		protected virtual ContainerWrapperCollection GetTranshipmentContainers() => new ContainerWrapperCollection(Factory);

		public FreightWrapperFromConsol TranshipmentFreightConsol
		{
			get { return fTranshipmentFreightConsol ?? (fTranshipmentFreightConsol = GetTranshipmentFreightConsol()); }
		}
		FreightWrapperFromConsol fTranshipmentFreightConsol;
		protected virtual FreightWrapperFromConsol GetTranshipmentFreightConsol() => null;

		public ZString CustomerReferenceNumber
		{
			get { return GetCustomerReferenceNumber(); }
		}
		protected virtual ZString GetCustomerReferenceNumber()
		{
			var number = CustomsEntries.GetCustomsEntryNumberSummaryForType(TransportAdditionalReferenceTypes.Codes.ClientReferenceNumber);
			if (string.IsNullOrEmpty(number))
			{
				number = CustomsEntries.GetCustomsEntryNumberSummaryForType(TransportAdditionalReferenceTypes.Descriptions.ClientReferenceNumber);
			}
			return number;
		}

		public DocARInvoice ARInvoice
		{
			get { return arInvoice; }
		}
		DocARInvoice arInvoice;

		public DocNettingStatement NettingStatement
		{
			get { return nettingStatement; }
		}
		DocNettingStatement nettingStatement;

#if DEBUG
		public void SetARInvoice(DocARInvoice arInvoice)
		{
			this.arInvoice = arInvoice;
		}
#endif

		public DocGenericTransactionHeader GenericTransactionHeader
		{
			get { return genericTransactionHeader; }
		}
		DocGenericTransactionHeader genericTransactionHeader;

		protected override void SetAdditionalCopyInfoCore(DocWrapperCopyInfo additionalCopyInfo)
		{
			if (ARInvoice != null)
			{
				this.ARInvoice.SetAdditionalCopyInfo(additionalCopyInfo);
			}
		}

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return ARInvoice != null ? (BusinessObject)ARInvoice.WrappedObject : base.BusinessObjectToLogAgainst; }
		}

		NotClearedByAgentWrapper NotClearedByAgent
		{
			get { return fNotClearedByAgent ?? (fNotClearedByAgent = GetNotClearedByAgent()); }
		}
		NotClearedByAgentWrapper fNotClearedByAgent;
		protected virtual NotClearedByAgentWrapper GetNotClearedByAgent()
		{
			return new NotClearedByAgentWrapper(null);
		}

		public ZString NotClearedByAgentStatement
		{
			get { return NotClearedByAgent.Statement; }
		}

		public ZString NotClearedByAgentNumber
		{
			get { return NotClearedByAgent.Number; }
		}

		public ZDateTime NotClearedByAgentIssueDate
		{
			get { return NotClearedByAgent.IssueDate; }
		}

		public ZDateTime NotClearedByAgentExpiryDate
		{
			get { return NotClearedByAgent.ExpiryDate; }
		}

		public ZString DepartureOrArrivalText
		{
			get
			{
				var text = ZString.Empty;
				if (DocumentDirection == "DEP")
				{
					if (ShipmentTransportMode.Code == Constants.TransportModes.Air)
					{
						text = Res.GetString("FreightWrapper|Departure|AIR", "Departure");
					}
					else
					{
						text = Res.GetString("FreightWrapper|Departure", "Departure");
					}
				}
				else if (DocumentDirection == "ARV")
				{
					text = Res.GetString("FreightWrapper|Arrival", "Arrival");
				}
				return text;
			}
		}

		public ZBool IsPODRequired
		{
			get { return GetIsPODRequired(); }
		}

		protected virtual ZBool GetIsPODRequired()
		{
			return false;
		}

		public ZString EFreightStatus
		{
			get { return GetEFreightStatus(); }
		}

		protected virtual ZString GetEFreightStatus()
		{
			return ZString.Empty;
		}

		public ZString CarrierContractNumber
		{
			get { return GetCarrierContractNumber(); }
		}

		protected virtual ZString GetCarrierContractNumber()
		{
			return ZString.Empty;
		}

		public ZString WarehouseNextDischargePort
		{
			get { return GetWarehouseNextDischargePort(); }
		}
		protected virtual ZString GetWarehouseNextDischargePort() => ZString.Empty;

		#endregion

		#region Related Wrapper Properties

		public eDocWrapperCollection EDocs
		{
			get { return eDocs ?? (eDocs = GetEDocs()); }
		}
		eDocWrapperCollection eDocs;

		eDocWrapperCollection GetEDocs()
		{
			return eDocWrapperCollection.New(GetParentBOForNoteStorageEDocsAndDocData() as IDocManagerSupport, Factory);
		}

		#region CA specific properties

		public Customs.DocReleaseStatusCollection CAReleaseStatus
		{
			get { return GetCAReleaseStatus(); }
		}
		protected virtual Customs.DocReleaseStatusCollection GetCAReleaseStatus()
		{
			return new Customs.DocReleaseStatusCollection(Factory);
		}

		public ZString CAPreviousCCN
		{
			get { return GetCAPreviousCCN(); }
		}
		protected virtual ZString GetCAPreviousCCN()
		{
			return ZString.Empty;
		}

		public ZString CATransactionNo
		{
			get { return GetCATransactionNo(); }
		}
		protected virtual ZString GetCATransactionNo()
		{
			return ZString.Empty;
		}

		public bool CAHideCarrier
		{
			get { return GetCAHideCarrier(); }
		}
		protected virtual bool GetCAHideCarrier() => true;

		public ZString CACarrierName
		{
			get { return GetCACarrierName(); }
		}
		protected virtual ZString GetCACarrierName()
		{
			return ZString.Empty;
		}

		public ZString CAUSPortOfExit
		{
			get { return GetCAUSPortOfExit(); }
		}
		protected virtual ZString GetCAUSPortOfExit()
		{
			return ZString.Empty;
		}

		public ZDateTime DateOfFirstArrival
		{
			get { return GetDateOfFirstArrival(); }
		}
		protected virtual ZDateTime GetDateOfFirstArrival()
		{
			return ZDateTime.Empty;
		}

		public ZDateTime WarehouseReleaseDate
		{
			get { return GetWarehouseReleaseDate(); }
		}
		protected virtual ZDateTime GetWarehouseReleaseDate()
		{
			return ZDateTime.Empty;
		}

		#endregion

		public QueryClaimWrapper QueryClaim
		{
			get { return GetQueryClaim(); }
		}
		protected virtual QueryClaimWrapper GetQueryClaim() => null;

		public RouteWrapperCollection ConsolRoutes
		{
			get { return fConsolRoutes ?? (fConsolRoutes = GetConsolRoutes()); }
		}
		RouteWrapperCollection fConsolRoutes;
		protected virtual RouteWrapperCollection GetConsolRoutes() => new RouteWrapperCollection(Factory);

		public RouteWrapperCollection ShipmentRoutes
		{
			get { return fShipmentRoutes ?? (fShipmentRoutes = GetShipmentRoutes()); }
		}
		RouteWrapperCollection fShipmentRoutes;
		protected virtual RouteWrapperCollection GetShipmentRoutes() => new RouteWrapperCollection(Factory);

		public CommercialInvoiceWrapperCollection CommercialInvoices
		{
			get { return fCommercialInvoices ?? (fCommercialInvoices = GetCommercialInvoices()); }
		}
		CommercialInvoiceWrapperCollection fCommercialInvoices;
		protected virtual CommercialInvoiceWrapperCollection GetCommercialInvoices() => new CommercialInvoiceWrapperCollection(Factory);

		public CommercialInvoiceLineWrapperCollection CommercialInvoiceLines
		{
			get { return fCommercialInvoiceLines ?? (fCommercialInvoiceLines = GetCommercialInvoiceLines()); }
		}
		CommercialInvoiceLineWrapperCollection fCommercialInvoiceLines;
		protected virtual CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines() => new CommercialInvoiceLineWrapperCollection(Factory);

		public DocJobChargeCollection FreightJobsChargesForOverseasAgent => GetFreightJobsChargesForOverseasAgent();

		protected virtual DocJobChargeCollection GetFreightJobsChargesForOverseasAgent() =>
			DocJobChargeCollection.GetCollection(this, nameof(FreightJobsChargesForOverseasAgent),
				(docCharges) =>
				{
					foreach (FreightWrapper jobWrapper in FreightJobs)
					{
						if (jobWrapper.JobHeader != null)
						{
							foreach (DocJobCharge charge in jobWrapper.JobHeader.JobChargesForAgentCollect)
							{
								if (charge.OSSellAmt != 0 && charge.OSSellCurrency != null)
								{
									docCharges.Add(charge);
								}
							}
						}
					}
				});

		public DocJobChargeCollection FreightJobsChargesForLocalClient => GetFreightJobsChargesForLocalClient();

		protected virtual DocJobChargeCollection GetFreightJobsChargesForLocalClient() =>
			DocJobChargeCollection.GetCollection(this, nameof(FreightJobsChargesForLocalClient),
				(docCharges) =>
				{
					foreach (FreightWrapper jobWrapper in FreightJobs)
					{
						if (jobWrapper.JobHeader != null)
						{
							foreach (DocJobCharge charge in jobWrapper.JobHeader.JobChargesForLocalClient)
							{
								if (charge.LocalSellAmount != 0)
								{
									docCharges.Add(charge);
								}
							}
						}
					}
				});

		public FreightWrapperCollection FreightJobs
		{
			get { return fFreightJobs ?? (fFreightJobs = GetFreightJobs()); }
		}
		FreightWrapperCollection fFreightJobs;
		protected virtual FreightWrapperCollection GetFreightJobs() => new FreightWrapperCollection(Factory);

		public FreightWrapper ParentJob
		{
			get { return fParentJob ?? (fParentJob = GetParentJob()); }
		}
		FreightWrapper fParentJob;

		protected virtual FreightWrapper GetParentJob()
		{
			return new FreightWrapperFromShipment(Factory.GetNull<ForwardingShipment>(), Factory);
		}

		public virtual AutoRateInfoWrapperCollection AutoRatedInfosForJobRevenue
		{
			get
			{
				if (autoRatedInfosForJobRevenue == null)
				{
					autoRatedInfosForJobRevenue = new AutoRateInfoWrapperCollection(Job, Factory);
				}
				return autoRatedInfosForJobRevenue;
			}
		}
		AutoRateInfoWrapperCollection autoRatedInfosForJobRevenue;

		#region WarehouseJob

		public WarehouseJobGenericWrapper WarehouseJob
		{
			get { return warehouseJob ?? (warehouseJob = GetWarehouseJob()); }
		}
		WarehouseJobGenericWrapper warehouseJob;
		protected virtual WarehouseJobGenericWrapper GetWarehouseJob() => null;

		#endregion

		public RunSheetWrapper RunSheet
		{
			get { return GetRunSheet(); }
		}
		protected virtual RunSheetWrapper GetRunSheet() => null;

		public ZString CustomsEntryNumber
		{
			get
			{
				if (fCustomsEntryNumber == ZString.Empty)
				{
					fCustomsEntryNumber = GetCustomsEntryNumber();
				}
				return fCustomsEntryNumber;
			}
		}
		ZString fCustomsEntryNumber;

		protected virtual ZString GetCustomsEntryNumber()
		{
			return ZString.Empty;
		}

		public FreightWrapperCollection FreightConsolidations
		{
			get { return fFreightConsolidations ?? (fFreightConsolidations = GetFreightConsolidations()); }
		}
		FreightWrapperCollection fFreightConsolidations;
		protected virtual FreightWrapperCollection GetFreightConsolidations() => new FreightWrapperCollection(Factory);

		public CustomsEntryWrapperCollection CustomsEntries
		{
			get { return fCustomsEntries ?? (fCustomsEntries = GetCustomsEntries()); }
		}
		CustomsEntryWrapperCollection fCustomsEntries;
		protected virtual CustomsEntryWrapperCollection GetCustomsEntries() => new CustomsEntryWrapperCollection(Factory);

		public ContainerWrapperCollection Containers
		{
			get { return fContainers ?? (fContainers = GetContainers()); }
		}
		ContainerWrapperCollection fContainers;
		protected virtual ContainerWrapperCollection GetContainers() => new ContainerWrapperCollection(Factory);

		public PackProductWrapperCollection PackProducts
		{
			get { return fPackProducts ?? (fPackProducts = GetPackProducts()); }
		}
		PackProductWrapperCollection fPackProducts;

		protected virtual PackProductWrapperCollection GetPackProducts()
		{
			var result = new PackProductWrapperCollection(Factory);
			foreach (PackageWrapper package in Packages)
			{
				if (package != null && package.Products != null)
				{
					result.AddRange(package.Products);
				}
			}
			return result;
		}

		public RequiredDocumentsWrapperCollection RequiredDocuments
		{
			get { return fRequiredDocuments ?? (fRequiredDocuments = GetRequiredDocuments()); }
		}
		RequiredDocumentsWrapperCollection fRequiredDocuments;
		protected virtual RequiredDocumentsWrapperCollection GetRequiredDocuments() => new RequiredDocumentsWrapperCollection(Factory);

		public ServiceWrapperCollection Services
		{
			get { return fServices ?? (fServices = GetServices()); }
		}
		ServiceWrapperCollection fServices;
		protected virtual ServiceWrapperCollection GetServices() => new ServiceWrapperCollection(this, Factory);

		public PickupDeliveryConfirmationsWrapperCollection PickupDeliveryConfirmations
		{
			get { return fPickupDeliveryConfirmations ?? (fPickupDeliveryConfirmations = GetPickupDeliveryConfirmations()); }
		}
		PickupDeliveryConfirmationsWrapperCollection fPickupDeliveryConfirmations;
		protected virtual PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations() => new PickupDeliveryConfirmationsWrapperCollection(Factory);

		public LocalTransportLegWrapperCollection LocalTransportLegs
		{
			get { return fLocalTransportLegs ?? (fLocalTransportLegs = GetLocalTransportLegs()); }
		}
		LocalTransportLegWrapperCollection fLocalTransportLegs;
		protected virtual LocalTransportLegWrapperCollection GetLocalTransportLegs() => new LocalTransportLegWrapperCollection(Factory);

		public OrderWrapperCollection Orders
		{
			get { return fOrders ?? (fOrders = GetOrders()); }
		}
		OrderWrapperCollection fOrders;
		protected virtual OrderWrapperCollection GetOrders() => new OrderWrapperCollection(Factory);

		public OrderLineWrapperCollection OrderLines
		{
			get { return orderLines ?? (orderLines = GetOrderLines()); }
		}
		OrderLineWrapperCollection orderLines;
		protected virtual OrderLineWrapperCollection GetOrderLines() => new OrderLineWrapperCollection(Factory);

		public PackageWrapperCollection Packages
		{
			get { return fPackages ?? (fPackages = GetPackages()); }
		}
		PackageWrapperCollection fPackages;
		protected virtual PackageWrapperCollection GetPackages() => new PackageWrapperCollection(Factory);

		public ContainerPenaltyWrapperCollection ImportContainerPenalties
		{
			get { return importContainerPenalties ??= GetImportContainerPenalties(); }
		}
		ContainerPenaltyWrapperCollection importContainerPenalties;
		protected virtual ContainerPenaltyWrapperCollection GetImportContainerPenalties() => new ContainerPenaltyWrapperCollection(Factory);

		public ContainerPenaltyWrapperCollection ExportContainerPenalties
		{
			get { return exportContainerPenalties ??= GetExportContainerPenalties(); }
		}
		ContainerPenaltyWrapperCollection exportContainerPenalties;
		protected virtual ContainerPenaltyWrapperCollection GetExportContainerPenalties() => new ContainerPenaltyWrapperCollection(Factory);

		public ContainerPenaltyWrapperCollection ContainerPenalties
		{
			get { return containerPenalties ?? (containerPenalties = GetContainerPenalties()); }
		}
		ContainerPenaltyWrapperCollection containerPenalties;
		protected virtual ContainerPenaltyWrapperCollection GetContainerPenalties() => new ContainerPenaltyWrapperCollection(Factory);

		public CO2eEmissionWrapperCollection CO2eEmissions
		{
			get { return cO2eEmissions ?? (cO2eEmissions = GetCO2eEmissions()); }
		}
		CO2eEmissionWrapperCollection cO2eEmissions;
		protected virtual CO2eEmissionWrapperCollection GetCO2eEmissions() => new CO2eEmissionWrapperCollection(Factory);

		public YardUnitWrapperCollection YardUnits
		{
			get { return yardUnits ?? (yardUnits = GetYardUnits()); }
		}
		YardUnitWrapperCollection yardUnits;
		protected virtual YardUnitWrapperCollection GetYardUnits() => new YardUnitWrapperCollection(Factory);

		public ZInt DocumentNumber => GetDocumentNumber();
		protected virtual ZInt GetDocumentNumber() => ZInt.Zero;

		public ZInt DocumentTotal => GetDocumentTotal();
		protected virtual ZInt GetDocumentTotal() => ZInt.Zero;

		public ZInt UOMTypeNumber => GetUOMTypeNumber();
		protected virtual ZInt GetUOMTypeNumber() => ZInt.Zero;

		public ZInt UOMTypeTotal => GetUOMTypeTotal();
		protected virtual ZInt GetUOMTypeTotal() => ZInt.Zero;

		#region Shorts/Overs

		public ZInt Shorts
		{
			get { return GetShorts(); }
		}

		protected virtual ZInt GetShorts() { return ZInt.Zero; }

		public ZInt Overs
		{
			get { return GetOvers(); }
		}

		protected virtual ZInt GetOvers() { return ZInt.Zero; }

		#endregion

		public UNDGSubstanceWrapperCollection UNDGs
		{
			get { return fUNDGs ?? (fUNDGs = GetUNDGs()); }
		}
		UNDGSubstanceWrapperCollection fUNDGs;
		protected virtual UNDGSubstanceWrapperCollection GetUNDGs() => new UNDGSubstanceWrapperCollection(Factory);

		public ZString UNDGsSummary => GetUNDGsSummary();

		protected virtual ZString GetUNDGsSummary() { return ZString.Empty; }

		public ExchangeRateWrapperCollection ExchangeRates
		{
			get { return exchangeRates ?? (exchangeRates = GetExchangeRates()); }
		}
		ExchangeRateWrapperCollection exchangeRates;
		protected virtual ExchangeRateWrapperCollection GetExchangeRates()
		{
			ExchangeRate[] rates = null;

			if (Job != null)
			{
				rates = Factory.Load<ExchangeRate>(new ZQuery(JobExRateSchema.JF_JH, Job.PK));
			}

			return new ExchangeRateWrapperCollection(rates, Factory);
		}

		public ZDateTime BillingDate
		{
			get { return GetBillingDate(); }
		}
		protected virtual ZDateTime GetBillingDate() => ZDateTime.Empty;

		public ZDateTime StorageFromDate
		{
			get { return GetStorageFromDate(); }
		}
		protected virtual ZDateTime GetStorageFromDate() => ZDateTime.Empty;

		public ZDateTime StorageToDate
		{
			get { return GetStorageToDate(); }
		}
		protected virtual ZDateTime GetStorageToDate() => ZDateTime.Empty;

		public CostWrapperCollection Costs
		{
			get { return costs ?? (costs = GetCosts()); }
		}
		CostWrapperCollection costs;
		protected virtual CostWrapperCollection GetCosts() => new CostWrapperCollection(Factory);

		public ChargeWrapperCollection Charges
		{
			get { return charges ?? (charges = GetCharges()); }
		}
		ChargeWrapperCollection charges;

		protected virtual ChargeWrapperCollection GetCharges()
		{
			return new ChargeWrapperCollection(Job, Factory);
		}

		public ChargeWrapperCollection NonCarrierCharges => nonCarrierCharges ?? (nonCarrierCharges = GetNonCarrierCharges());
		ChargeWrapperCollection nonCarrierCharges;

		protected virtual ChargeWrapperCollection GetNonCarrierCharges() => new ChargeWrapperCollection(Factory);

		public ChargeWrapperCollection CarrierCharges => carrierCharges ?? (carrierCharges = GetCarrierCharges());
		ChargeWrapperCollection carrierCharges;

		protected virtual ChargeWrapperCollection GetCarrierCharges() => new ChargeWrapperCollection(Factory);

		#region Milestones

		public WorkflowItemWrapperCollection Milestones
		{
			get { return milestones ?? (milestones = GetMilestones()); }
		}
		WorkflowItemWrapperCollection milestones;

		WorkflowItemWrapperCollection GetMilestones()
		{
			if (DocumentsDataRegistry.Instance.ShowMilestonesOnPODDocument.Value)
			{
				var workflowProvider = WrappedBO as IWorkflowProvider;
				if (workflowProvider != null)
				{
					return new WorkflowItemWrapperCollection(workflowProvider.WorkflowItems.MilestonesIncludingRelated, WrappedBO.Factory);
				}
			}
			return new WorkflowItemWrapperCollection(Factory);
		}

		#endregion

		#region BookingInstructions

		public InstructionWrapperCollection BookingInstructions
		{
			get { return bookingInstructions ?? (bookingInstructions = GetBookingInstructions()); }
		}

		InstructionWrapperCollection bookingInstructions;
		protected virtual InstructionWrapperCollection GetBookingInstructions() => new InstructionWrapperCollection(Factory);

		#endregion

		#region TransportBookings

		public FreightWrapperCollection TransportBookings
		{
			get { return transportBookings ?? (transportBookings = GetTransportBookings()); }
		}

		FreightWrapperCollection transportBookings;
		protected virtual FreightWrapperCollection GetTransportBookings() => new FreightWrapperCollection(Factory);

		#endregion

		#region TransportAddresses

		public AddressWrapperCollection TransportAddresses
		{
			get { return transportAddresses ?? (transportAddresses = GetTransportAddresses()); }
		}

		AddressWrapperCollection transportAddresses;
		protected virtual AddressWrapperCollection GetTransportAddresses() => new AddressWrapperCollection(Factory);

		#endregion

		#region TransportAddressesWithWarehousing

		public AddressWrapperCollection TransportAddressesWithWarehousing
		{
			get { return transportAddressesWithWarehousing ?? (transportAddressesWithWarehousing = GetTransportAddressesWithWarehousing()); }
		}

		AddressWrapperCollection transportAddressesWithWarehousing;

		AddressWrapperCollection GetTransportAddressesWithWarehousing()
		{
			return TransportAddresses.GetAddressesWithWarehousing();
		}

		#endregion

		#region GoodsHandlingInstructions

		public ZString GoodsHandlingInstructions
		{
			get { return GetGoodsHandlingInstructions(); }
		}

		protected virtual ZString GetGoodsHandlingInstructions()
		{
			return ZString.Empty;
		}

		#endregion

		#region GateTransport

		public GateTransportWrapper GateTransport
		{
			get { return gateTransport ?? (gateTransport = GetGateTransport()); }
		}
		GateTransportWrapper gateTransport;
		protected virtual GateTransportWrapper GetGateTransport() => new GateTransportWrapper(null, null, Factory);

		#endregion

		#endregion

		#region TrackingUrl
		public TrackingConstants.BusinessContext TrackingBusinessContext
		{
			get { return GetTrackingBusinessContext(); }
		}
		protected virtual TrackingConstants.BusinessContext GetTrackingBusinessContext() => TrackingConstants.BusinessContext.NoBusinessContext;

		public ZGuid TrackingBusinessObjectPK
		{
			get { return GetTrackingBusinessObjectPK(); }
		}
		protected virtual ZGuid GetTrackingBusinessObjectPK() => ZGuid.Empty;
		#endregion

		#region Branding

		DocJobHeader jobHeader;
		public DocJobHeader JobHeader
		{
			get { return jobHeader ?? (jobHeader = DocJobHeader.New(Job, Factory)); }
		}

		protected override ClientTariffAndLevel TariffAndLevelRegistry
		{
			get
			{
				ClientTariffAndLevel result = null;

				if (JobHeader != null)
				{
					var localCharges = JobHeader.LocalCharges;
					if (localCharges != null)
					{
						var tariffLevelCode = localCharges.MiscServ.ARGlobalRateBase.ToString();
						result = (ClientTariffAndLevel)DocumentsDataRegistry.Instance.ClientTariffAndLevels.Value.FindByCode(tariffLevelCode);
					}
				}
				else
				{
					result = base.TariffAndLevelRegistry;
				}

				return result;
			}
		}

		protected override Image GetCompanyLogoFallback()
		{
			var result = !SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.Value ? JobHeaderBranchLogo : null;
			return result ?? base.GetCompanyLogoFallback();
		}

		protected virtual Image JobHeaderBranchLogo
		{
			get
			{
				if (JobHeader != null && !JobHeader.JobHeader.IsDeleted && JobHeader.Branch != null)
				{
					var branchLogo = JobHeader.Branch.GetDepartmentBranchLogo(JobHeader.Department != null
						? JobHeader.Department.DepartmentPK.ToGuid()
						: Guid.Empty);

					return branchLogo;
				}

				return null;
			}
		}

		#endregion

		#region Barcodes

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				if (docManagerBarcode == null)
				{
					var iDocTypeCode = this as IDocTypeCode;
					if (iDocTypeCode != null && !DocManagerCode.IsEmpty)
					{
						docManagerBarcode = BarcodeGenerator.CreateDocumentBarcode(DocManagerCode, TextForBarcode, iDocTypeCode.DocTypeCode + ";");
					}
					else
					{
						docManagerBarcode = base.DocManagerBarcode;
					}
				}

				return docManagerBarcode;
			}
		}
		TextBarcode docManagerBarcode;

		protected TextBarcode JobNumberBarcode
		{
			get
			{
				if (jobNumberBarcode == null)
				{
					var iDocTypeCode = this as IDocTypeCode;
					if (iDocTypeCode != null && !DocManagerCode.IsEmpty)
					{
						jobNumberBarcode = BarcodeGenerator.CreateDocumentBarcode(DocManagerCode, JobNumber, iDocTypeCode.DocTypeCode + ";");
					}
				}

				return jobNumberBarcode;
			}
		}

		TextBarcode jobNumberBarcode;

		public ZString JobNumberBarcodeTextForFont
		{
			get { return JobNumberBarcode != null ? JobNumberBarcode.TextAs128sFontString : ZString.Empty; }
		}

		public ZString JobNumberBarcodeText
		{
			get { return JobNumberBarcode != null ? JobNumberBarcode.TextToEncode : ZString.Empty; }
		}

		public ZString JobNumberBarcodeTextWithoutDocManagerCodes
		{
			get
			{
				return new TextBarcode(JobNumber).TextAs128sFontString;
			}
		}

		BarcodeGenerator BarcodeGenerator
		{
			get { return barcodeGenerator ?? (barcodeGenerator = new BarcodeGenerator()); }
		}

		BarcodeGenerator barcodeGenerator;

		#endregion

		#region IDocTypeCode Members

		ZString IDocTypeCode.DocTypeCode
		{
			get;
			set;
		}

		#endregion

		bool IShouldExcludeFromDocPackByDefault.IsExcluded => ARInvoice != null && ((IShouldExcludeFromDocPackByDefault)ARInvoice).IsExcluded;

		#region IDocWrapperWithChildBizObjToWrap Members

		BusinessObject IDocWrapperWithChildBizObjToWrap.WrappedBusinessObject => BusinessObjectToLogAgainst;

		BusinessObject IDocWrapperWithChildBizObjToWrap.WrappedChildBusinessObject => wrappedChildBusinessObject;

		BusinessObject wrappedChildBusinessObject;

		#endregion

		#region NCTS

		public CodeAndDescriptionWrapper NCTSDeclarationType => GetNCTSDeclarationType();

		protected virtual CodeAndDescriptionWrapper GetNCTSDeclarationType() => CodeAndDescriptionWrapper.Empty;

		public ZString NCTSDepartureTransportID => GetNCTSDepartureTransportID();

		protected virtual ZString GetNCTSDepartureTransportID() => ZString.Empty;

		public CodeAndDescriptionWrapper NCTSDepartureTransportCountry => GetNCTSDepartureTransportCountry();

		protected virtual CodeAndDescriptionWrapper GetNCTSDepartureTransportCountry() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper NCTSDepartureTransportMode => GetNCTSDepartureTransportMode();

		protected virtual CodeAndDescriptionWrapper GetNCTSDepartureTransportMode() => CodeAndDescriptionWrapper.Empty;

		public ZString NCTSFrontierTransportID => GetNCTSFrontierTransportID();

		protected virtual ZString GetNCTSFrontierTransportID() => ZString.Empty;

		public CodeAndDescriptionWrapper NCTSFrontierTransportCountry => GetNCTSFrontierTransportCountry();

		protected virtual CodeAndDescriptionWrapper GetNCTSFrontierTransportCountry() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper NCTSFrontierTransportMode => GetNCTSFrontierTransportMode();

		protected virtual CodeAndDescriptionWrapper GetNCTSFrontierTransportMode() => CodeAndDescriptionWrapper.Empty;

		public ZString NCTSGoodsLocationCode => GetNCTSGoodsLocationCode();

		protected virtual ZString GetNCTSGoodsLocationCode() => ZString.Empty;

		public ZString NCTSGoodsLocation => GetNCTSGoodsLocation();

		protected virtual ZString GetNCTSGoodsLocation() => ZString.Empty;

		public CodeAndDescriptionWrapper NCTSDepartureOffice => GetNCTSDepartureOffice();

		protected virtual CodeAndDescriptionWrapper GetNCTSDepartureOffice() => CodeAndDescriptionWrapper.Empty;

		public CodeAndDescriptionWrapper NCTSDestinationOffice => GetNCTSDestinationOffice();

		protected virtual CodeAndDescriptionWrapper GetNCTSDestinationOffice() => CodeAndDescriptionWrapper.Empty;

		#endregion

		enum TaxNumberType
		{
			Shipper,
			Consignee,
			NotifyParty
		}
	}
}
