using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ToBillOfLadingConverter : FlatFileConverter
	{
		public ToBillOfLadingConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory) { }

		public static class PaymentTerm
		{
			public const string Prepaid = "PPD";
			public const string Collect = "CLT";
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.AgencyBillsOfLading bills = (Xsd.AgencyBillsOfLading)valueObject;
			try
			{
				new ToBillOfLadingMapper(bills.AgencyBillOfLading, fileLines.GetEnumerator()).Process();
			}
			catch (ToBillOfLadingMapper.ProcessingException ex)
			{
				Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, ex.Message));
			}
		}

		class ToBillOfLadingMapper
		{
			public ToBillOfLadingMapper(Xsd.AgencyBillOfLadingCollection bolCollection, IEnumerator rowEnumerator)
			{
				this.bolCollection = bolCollection;
				this.rowEnumerator = rowEnumerator;
			}

			#region Processing

			public void Process()
			{
				NextRow();
				if (RowType != ShipnetConstants.Codes.RecordID.FileHeader)
				{
					throw new ProcessingException("No File Header record at the start of the file.");
				}

				string vesselName = row[ShipnetConstants.FileHeader.VesselName];
				string lloydsNumber = row[ShipnetConstants.FileHeader.VesselLloyds];
				string voyageNumber = row[ShipnetConstants.FileHeader.VoyageNumber];

				NextRow();

				while (RowType == ShipnetConstants.Codes.RecordID.BookingHeader)
				{
					Xsd.AgencyBillOfLading bol = bolCollection.AddNew();

					bol.Sailing.VesselName = vesselName;
					bol.Sailing.VoyageNo = voyageNumber;
					bol.Sailing.LloydsNo = lloydsNumber;
					bol.Sailing.ETA = row.GetFieldAsZDateTime(ShipnetConstants.BookingHeader.ETA, "yyyyMMdd");
					bol.Sailing.ETD = row.GetFieldAsZDateTime(ShipnetConstants.BookingHeader.ETD, "yyyyMMdd");
					bol.BillNumber = row[ShipnetConstants.BookingHeader.ReferenceNo];
					bol.CargoType = GetContainerMode(row[ShipnetConstants.BookingHeader.EDIFACTCargoType]);
					bol.Load.Value = row[ShipnetConstants.BookingHeader.LoadPortCode];
					bol.Origin.Value = row[ShipnetConstants.BookingHeader.AcceptancePortCode];
					bol.Discharge.Value = row[ShipnetConstants.BookingHeader.DischargePortCode];
					bol.Destination.Value = row[ShipnetConstants.BookingHeader.DeliveryPortCode];
					bol.ReleaseType = Xsd.AgencyReleaseType.OBR;
					bol.INCOTerm = GetPaymentTerm(row[ShipnetConstants.BookingHeader.PaymentTerms]);

					NextRow();

					if (RowType == ShipnetConstants.Codes.RecordID.ShipperDetails)
					{
						AddAddress(Xsd.DocAddressAddressType.CRD, bol.Addresses.DocAddress);
						NextRow();
					}

					if (RowType == ShipnetConstants.Codes.RecordID.ConsigneeDetails)
					{
						AddAddress(Xsd.DocAddressAddressType.CED, bol.Addresses.DocAddress);
						NextRow();
					}

					if (RowType == ShipnetConstants.Codes.RecordID.NotifyPartyDetails)
					{
						AddAddress(Xsd.DocAddressAddressType.NPP, bol.Addresses.DocAddress);
						NextRow();
					}

					if (RowType == ShipnetConstants.Codes.RecordID.UltimateConsigneeDetails)
					{
						AddAddress(Xsd.DocAddressAddressType.CEG, bol.Addresses.DocAddress);
						NextRow();
					}

					if (RowType == ShipnetConstants.Codes.RecordID.FreightForwarderDetails)
					{
						NextRow();
					}

					if (RowType == ShipnetConstants.Codes.RecordID.CustomerDetails)
					{
						AddAddress(Xsd.DocAddressAddressType.BKD, bol.Addresses.DocAddress);
						NextRow();
					}

					while (RowType == ShipnetConstants.Codes.RecordID.CargoGroup)
					{
						bol.PackageSummary.Weight.DimensionType = Core.Constants.Weight.Kilograms;
						bol.PackageSummary.Weight.Value = row.GetFieldAsZDecimal(ShipnetConstants.CargoGroup.TotalGrossWeightKGS);
						bol.PackageSummary.NumberOfPacks = row.GetFieldAsUInt(ShipnetConstants.CargoGroup.TotalNoOfPackages);
						bol.PackageSummary.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
						bol.PackageSummary.Volume.Value = row.GetFieldAsZDecimal(ShipnetConstants.CargoGroup.TotalGrossCubeCBM);

						NextRow();

						if (RowType == ShipnetConstants.Codes.RecordID.CargoGroupDescriptions)
						{
							bol.Description = row[ShipnetConstants.CargoDescriptions.Description];
							bol.MarksAndNumbers = row[ShipnetConstants.CargoDescriptions.Marks];
							NextRow();
						}

						while (RowType == ShipnetConstants.Codes.RecordID.CargoGroupItems)
						{
							if (bol.PackageSummary.PackType.IsEmpty)
							{
								ZString result = SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit(row[ShipnetConstants.CargoGroupItems.TypeOfPackages]);
								if (result.IsEmpty)
								{
									result = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
								}

								bol.PackageSummary.PackType = result;
							}

							Xsd.Package tempPackage = new Xsd.Package();

							tempPackage.NumberOfPacks = row.GetFieldAsUInt(ShipnetConstants.CargoGroupItems.NumberOfPackages);
							tempPackage.PackType = SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit(row[ShipnetConstants.CargoGroupItems.TypeOfPackages]);
							tempPackage.Weight.DimensionType = Core.Constants.Weight.Kilograms;
							tempPackage.Weight.Value = row.GetFieldAsZDecimal(ShipnetConstants.CargoGroupItems.GrossWeightKGS);
							tempPackage.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
							tempPackage.Volume.Value = row.GetFieldAsZDecimal(ShipnetConstants.CargoGroupItems.GrossCubeCBM);
							tempPackage.GoodsDescription = row[ShipnetConstants.CargoGroupItems.CargoDescription];

							NextRow();

							if (RowType == ShipnetConstants.Codes.RecordID.CargoItemDescriptions)
							{
								tempPackage.GoodsDescription = row[ShipnetConstants.CargoDescriptions.Description];
								tempPackage.MarksAndNumbers = row[ShipnetConstants.CargoDescriptions.Marks];

								NextRow();
							}

							if (RowType == ShipnetConstants.Codes.RecordID.EquipmentDetails)
							{
								while (RowType == ShipnetConstants.Codes.RecordID.EquipmentDetails)
								{
									bol.CargoType = Xsd.ContainerMode.FCL;

									Xsd.Container container = bol.Containers.AddNew();

									container.PackingMode = Xsd.ContainerMode.FCL;
									container.ContainerNumber = row[ShipnetConstants.EquipmentDetails.EquipmentNumber];
									container.ContainerType.ContainerCode = row[ShipnetConstants.EquipmentDetails.SizeType];
									container.Weight = row.GetFieldAsZDecimal(ShipnetConstants.EquipmentDetails.TotalTareWeight);
									container.Seal = row[ShipnetConstants.EquipmentDetails.SealNumber1];
									container.IsEmptyContainer = (row[ShipnetConstants.EquipmentDetails.EmptyFull] == ShipnetConstants.Codes.EmptyFull.Empty);

									switch (row[ShipnetConstants.EquipmentDetails.ShippersOwn])
									{
										case "Y":
											container.IsShipperOwnedContainer = true;
											container.IsShipperOwnedContainerSpecified = true;
											break;

										case "N":
											container.IsShipperOwnedContainer = false;
											container.IsShipperOwnedContainerSpecified = true;
											break;

										default:
											container.IsShipperOwnedContainerSpecified = false;
											break;
									}

									if (!container.IsEmptyContainer)
									{
										Xsd.Package package = bol.Packages.AddNew();

										package.ContainerNumber = container.ContainerNumber;
										package.NumberOfPacks = row.GetFieldAsUInt(ShipnetConstants.EquipmentDetails.NoOfPackages);
										package.PackType = SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit(row[ShipnetConstants.EquipmentDetails.Packages]);
										package.Weight.DimensionType = Core.Constants.Weight.Kilograms;
										package.Weight.Value = row.GetFieldAsZDecimal(ShipnetConstants.EquipmentDetails.TotalGrossWeightKGS) - container.Weight;
										package.Volume.DimensionType = Core.Constants.Volume.CubicMetres;
										package.Volume.Value = row.GetFieldAsZDecimal(ShipnetConstants.EquipmentDetails.TotalGrossCubeCBM);

										package.GoodsDescription = tempPackage.GoodsDescription;
										package.MarksAndNumbers = tempPackage.MarksAndNumbers;
									}

									NextRow();
								}
							}
							else
							{
								// If we don't have any EQD lines, we want to use this data
								bol.Packages.Add(tempPackage);
							}
						}
					}

					if (RowType == ShipnetConstants.Codes.RecordID.ChargeDetail)
					{
						Xsd.Billing billing = bol.Billing;

						while (RowType == ShipnetConstants.Codes.RecordID.ChargeDetail)
						{
							Xsd.ChargeLine chargeLine = billing.ChargeLines.AddNew();
							chargeLine.ChargeCode = row[ShipnetConstants.ChargeDetail.ChargeCode];
							chargeLine.Collect = row[ShipnetConstants.ChargeDetail.PrepaidCollect] == "C";
							chargeLine.OSSellAmount.CurrencyCode = row[ShipnetConstants.ChargeDetail.Currency];
							chargeLine.OSSellAmount.Value = row.GetFieldAsZDecimal(ShipnetConstants.ChargeDetail.Amount);

							NextRow();
						}
					}
				}

				if (RowType != ShipnetConstants.Codes.RecordID.Summary)
				{
					throw new ProcessingException("Invalid row or missing summary row: " + RowType);
				}

				// Do something with summary row?
			}

			Xsd.ContainerMode GetContainerMode(string edifactCargoType)
			{
				Xsd.ContainerMode result = Xsd.ContainerMode.FCL;
				switch (edifactCargoType)
				{
					case ShipnetConstants.Codes.EDIFACTCargoType.BB:
						result = Xsd.ContainerMode.BBK;
						break;

					case ShipnetConstants.Codes.EDIFACTCargoType.FCL:
					case ShipnetConstants.Codes.EDIFACTCargoType.LCLFCL:
						result = Xsd.ContainerMode.FCL;
						break;

					case ShipnetConstants.Codes.EDIFACTCargoType.LCLLCL:
						result = Xsd.ContainerMode.LCL;
						break;

					case ShipnetConstants.Codes.EDIFACTCargoType.U:
						result = Xsd.ContainerMode.OTH;
						break;
				}

				return result;
			}

			public string GetPaymentTerm(ZString paymentTerm)
			{
				string result = string.Empty;
				switch (paymentTerm)
				{
					case ShipnetConstants.Codes.PaymentType.Prepaid:
						result = PaymentTerm.Prepaid;
						break;
					case ShipnetConstants.Codes.PaymentType.Collect:
						result = PaymentTerm.Collect;
						break;
					default:
						break;
				}

				return result;
			}

			void AddAddress(Xsd.DocAddressAddressType addressType, Xsd.DocAddressCollection addresses)
			{
				Xsd.DocAddress address = addresses.AddNew();
				address.AddressType = addressType;
				address.AddressTypeSpecified = true;
				address.AddressCode = row[ShipnetConstants.PartyDetails.PartyCode];
				address.CompanyName = row[ShipnetConstants.PartyDetails.PartyName];
				address.AddressLine1 = row[ShipnetConstants.PartyDetails.PartyAddress1];

				if (!address.CompanyName.ContainsAnyLetters && !ContainsNumbers(address.CompanyName))
				{
					string err = "Company name is invalid on line " + currentIndex + ": \"" + row.InitialRawRow.TrimEnd() + "\"";
					throw new ProcessingException(err);
				}

				ZStringBuilder builder = new ZStringBuilder();
				ZString[] addrs = {
					row[ShipnetConstants.PartyDetails.PartyAddress2],
					row[ShipnetConstants.PartyDetails.PartyAddress3],
					row[ShipnetConstants.PartyDetails.PartyAddress4],
					row[ShipnetConstants.PartyDetails.PartyAddress5]
				};

				foreach (ZString addr in addrs)
				{
					if (!addr.IsEmpty)
					{
						builder.Append(addr);
					}
				}

				address.AddressLine2 = builder.ToStringWithDelimiterBetweenAppends(" ");

				address.AddressReference.Organisation.OwnerCode = address.AddressCode;
				address.AddressReference.Organisation.OrganisationDetails.Name = address.CompanyName;
				{
					Xsd.OrgAddress orgAddress = address.AddressReference.Organisation.OrganisationDetails.Addresses.AddNew();
					orgAddress.AddressLine1 = address.AddressLine1;
					orgAddress.AddressLine2 = address.AddressLine2;
				}
			}

			bool ContainsNumbers(ZString s)
			{
				foreach (char ch in s)
				{
					if (char.IsDigit(ch))
					{
						return true;
					}
				}
				return false;
			}

			#endregion

			#region Helpers

			string RowType { get { return row[ShipnetConstants.Common.RecordID]; } }

			#endregion

			#region Implementation

			[Serializable]
			public class ProcessingException : ApplicationException
			{
				public ProcessingException(string message)
					: base(message)
				{
				}

#if NETFRAMEWORK
				protected ProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
					: base(info, context)
				{ }
#endif
			}

			BaseDataRow row;

			int currentIndex;

			void NextRow()
			{
				if (rowEnumerator.MoveNext())
				{
					row = (BaseDataRow)rowEnumerator.Current;
					currentIndex++;
				}
				else
				{
					throw new ProcessingException("Import file is missing lines. Check that there is a summary record at the end of the file.");
				}
			}

			readonly IEnumerator rowEnumerator;
			readonly Xsd.AgencyBillOfLadingCollection bolCollection;

			#endregion
		}
	}
}
