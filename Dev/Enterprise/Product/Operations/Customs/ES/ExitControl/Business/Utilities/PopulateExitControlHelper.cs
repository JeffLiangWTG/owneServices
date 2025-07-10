using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class PopulateExitControlHelper
	{
		public (IEnumerable<CusExitHeader>, IEnumerable<ZString> mrnsNotAssociated, IEnumerable<ZString> errorMessages) GetOrCreateExitHeader(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries)
		{
			var headersToReturn = new List<CusExitHeader>();
			var existingNotChildrenMessageList = new List<ZString>();
			var existingNotChildrenMRNList = new List<ZString>();
			var parentBusinessObject = declaration.Shipment as BusinessObject ?? declaration;

			foreach (var entry in acceptedEntries)
			{
				var mrn = entry.MovementReferenceNumber;
				var exitHeaders = GetHeaderForConsignmentWithMRN(mrn, declaration.Factory);

				if (!exitHeaders.IsNullOrEmpty())
				{
					if (exitHeaders.Length == 1 && exitHeaders[0].CXH_ParentID == parentBusinessObject.PK)
					{
						var header = exitHeaders[0];
						if (!headersToReturn.Contains(header))
						{
							FillExitHeaderData(header, declaration);
							headersToReturn.Add(header);
						}
					}
					else
					{
						existingNotChildrenMRNList.Add(mrn);
						exitHeaders.ForEach(x => existingNotChildrenMessageList.Add(x.CXH_ParentID.IsEmpty ? GetExitControlMessage(mrn, x.CXH_JobReference) : (x.Parent is JobDeclaration dec) ? GetRefNumMessage(mrn, dec.JE_DeclarationReference) : GetExitControlMessage(mrn, x.CXH_JobReference)));
					}
				}
			}

			if (headersToReturn.IsNullOrEmpty() && existingNotChildrenMessageList.Count != acceptedEntries.Count())
			{
				headersToReturn.Add(GetHeaderForDeclaration(declaration, parentBusinessObject));
			}

			return (headersToReturn, existingNotChildrenMRNList, existingNotChildrenMessageList);
		}

		ZString GetRefNumMessage(ZString mrn, ZString reference) => ResString.GetMultilingualString("2F3C99F9-D78D-49A3-90B2-475F0BB2F14D", "A movement for MRN {0} already exists in Job Number {1}", mrn, reference);
		ZString GetExitControlMessage(ZString mrn, ZString reference) => ResString.GetMultilingualString("831A2D3F-F179-4E53-A89C-E19B3AF325FA", "A movement for MRN {0} already exists in Exit Control {1}", mrn, reference);

		void FillExitHeaderData(CusExitHeader exitHeader, JobDeclaration declaration)
		{
			if (declaration.ShippingLine != null)
			{
				exitHeader.CXH_OA_Carrier = declaration.ShippingLine.MainAddress.PK;
			}

			if (declaration.Supplier != null)
			{
				exitHeader.CXH_OH_Exporter = declaration.JE_OH_Supplier;
			}

			if (exitHeader.CXH_GS_NKCustomsAgent.IsEmpty && !GlbStaff.CurrentUser.GS_IsSystemAccount)
			{
				exitHeader.CXH_GS_NKCustomsAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}

		CusExitHeader[] GetHeaderForConsignmentWithMRN(ZString mrn, BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			var subQuery1 = new ZDBOnlyQuery(typeof(CusExitHeader));
			var subQuery2 = new ZDBOnlySubQuery(typeof(CusExitConsignment), CusExitConsignmentSchema.CXC_CXH_Header);
			subQuery2.AddToFilter(CusExitConsignmentSchema.CXC_MovementReference, mrn);
			subQuery1.AddSubQuery(subQuery2, JoinCondition.And);
			query.AddToFilter(subQuery1);
			return factory.Load<CusExitHeader>(query);
		}

		CusExitHeader GetHeaderForDeclaration(JobDeclaration declaration, BusinessObject parentBusinessObject)
		{
			CusExitHeader headerToReturn;

			var query = new ZQuery(CusExitHeaderSchema.CXH_ParentID, parentBusinessObject.PK);
			var exitHeaders = declaration.Factory.Load<CusExitHeader>(query);
			if (exitHeaders != null && exitHeaders.Length > 0)
			{
				headerToReturn = declaration.Factory.Load<CusExitHeader>(exitHeaders[0].PK);
			}
			else
			{
				var exitHeader = declaration.Factory.New<CusExitHeader>();
				exitHeader.CXH_ParentID = parentBusinessObject.PK;
				exitHeader.CXH_ParentTableCode = parentBusinessObject.TablePrefix;
				exitHeader.CXH_JobReference = declaration.JE_DeclarationReference;
				headerToReturn = exitHeader;
			}

			FillExitHeaderData(headerToReturn, declaration);
			return headerToReturn;
		}

		public int GenerateExitControlFromEntries(IEnumerable<Tuple<ZString, ZString>> selectedMRNAndRefNums, CusExitHeader exitHeader, JobDeclaration declaration)
		{
			AddContainers(exitHeader, declaration);
			AddEquipments(exitHeader, declaration);

			var reportsAddedOrUpdated = 0;

			exitHeader.TrainingEntry = declaration.ZG_IsTrainingDeclaration;

			foreach (var (mrn, refNum) in selectedMRNAndRefNums)
			{
				var exitConsignments = exitHeader.CusExitConsignments;
				var consignment = exitConsignments.FirstOrDefault(x => x.CXC_MovementReference == mrn);
				if (consignment == null)
				{
					consignment = exitHeader.CusExitConsignments.AddNew();
					consignment.CXC_MovementReference = mrn;
				}
				AddCusExitConsignmentItems(declaration, consignment, mrn);
				consignment.CXC_LocalReference = refNum;

				var report = exitHeader.CusExitReports.FirstOrDefault(r => r.CER_CXC_Consignment == consignment.PK);
				if (report == null)
				{
					report = exitHeader.CusExitReports.AddNew();
					report.CER_CXC_Consignment = consignment.PK;
				}
				report.CER_OfficeOfExit = declaration.GetExitCustomsOffice();
				report.CER_TransportMode = declaration.JE_TransportModeInland;

				reportsAddedOrUpdated++;
			}

			return reportsAddedOrUpdated;
		}

		void AddContainers(CusExitHeader exitHeader, JobDeclaration declaration)
		{
			foreach (var container in declaration.CusContainers.Where(x => !x.CO_ContainerNumber.IsEmpty))
			{
				var containerAlreadyAdded = exitHeader.CusExitContainers.Any(x => x.CXN_ContainerNumber == container.CO_ContainerNumber);
				if (!containerAlreadyAdded)
				{
					var newContainer = exitHeader.CusExitContainers.AddNew();
					newContainer.CXN_ContainerNumber = container.CO_ContainerNumber.Left(CusExitContainer.Schema.CXN_ContainerNumberMaxLength);
					newContainer.CXN_IsEquipment = false;
					if (!container.CO_Seal.IsEmpty)
					{
						newContainer.AllSealNumbers.AddNew().BK_SealNumber = container.CO_Seal;
					}

					if (!container.CO_SecondSeal.IsEmpty)
					{
						newContainer.AllSealNumbers.AddNew().BK_SealNumber = container.CO_SecondSeal;
					}

					container.AdditionalSeals.ForEach(s => newContainer.AllSealNumbers.AddNew().BK_SealNumber = s.BK_SealNumber);
				}
			}
		}

		void AddEquipments(CusExitHeader exitHeader, JobDeclaration declaration)
		{
			foreach (var equipment in declaration.Equipments.Where(x => !x.CEQ_IdentificationNumber.IsEmpty))
			{
				var equipmentAlreadyAdded = exitHeader.CusExitContainers.Any(x => x.CXN_ContainerNumber == equipment.CEQ_IdentificationNumber);
				if (!equipmentAlreadyAdded)
				{
					var newContainer = exitHeader.CusExitContainers.AddNew();
					newContainer.CXN_ContainerNumber = equipment.CEQ_IdentificationNumber.Substring(0, Customs.Business.AutoCusContainer.Schema.CO_ContainerNumberMaxLength);
					newContainer.CXN_IsEquipment = true;

					equipment.Seals.ForEach(s => newContainer.AllSealNumbers.AddNew().BK_SealNumber = s.BK_SealNumber);
				}
			}
		}

		void AddCusExitConsignmentItems(JobDeclaration declaration, CusExitConsignment consignment, string mrn)
		{
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.First(x => ((CusEntryHeader)x).MovementReferenceNumber == mrn);
			var consignmentItems = consignment.CusExitConsignmentItems;
			consignmentItems.DeleteAll();
			foreach (var entryLine in entryHeader.AllEntryLines.Cast<CusEntryLine>())
			{
				var consignmentItem = consignmentItems.AddNew();
				consignmentItem.CCI_LineNumber = entryLine.CL_LineNumber;
				consignmentItem.CCI_GrossMass = entryLine.TotalGrossWeightInKG;
				consignmentItem.CCI_NetMass = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.NetWeightInKG);
				consignmentItem.CCI_UniqueConsignmentReference = entryLine.RandomLine.ZG_CommercialReference.Substring(0, CusExitConsignmentItem.Schema.CCI_UniqueConsignmentReferenceMaxLength);
				AddPackages(consignmentItem, entryLine);
			}
		}

		void AddPackages(CusExitConsignmentItem item, CusEntryLine entryLine)
		{
			var framePckgType = ES.Business.UniversalReferenceConstants.RefCusCodeList.PackageType.Frame;
			var listOfVehicles = new List<ZString>();
			var listOfPackages = new Dictionary<ZString, Tuple<ZInt, ZString, ZString, ZString>>();
			foreach (var invLine in entryLine.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				if (invLine.Vehicles.Cast<CusVehicle>().Any())
				{
					const string vehicleSeparator = ":";
					string FormatForVehicle(ZString input) => vehicleSeparator + input;
					var vehiclesSeparator = "";
					var formattedVehicle = "";

					foreach (CusVehicle vehicle in invLine.Vehicles)
					{
						formattedVehicle += vehiclesSeparator + vehicle.CVH_VehicleIdentificationNumber + FormatForVehicle(vehicle.CVH_BrandName) + FormatForVehicle(vehicle.CVH_ModelName);
						vehiclesSeparator = ", ";
					}
					listOfVehicles.Add(formattedVehicle);

					invLine.PackagesForInvoiceLinesForBindingOnly.Where(p => p.Package.CW_PackType != framePckgType && p.IsLinked).ForEach(p => GetPackagesTuples(p, listOfPackages));
				}
				else
				{
					invLine.PackagesForInvoiceLinesForBindingOnly.Where(p => p.IsLinked).ForEach(p => GetPackagesTuples(p, listOfPackages));
				}
			}

			foreach (var vehicle in listOfVehicles)
			{
				var package = item.CusExitConsignmentPackagePivots.AddNew().Package;
				package.CXP_Quantity = 1;
				package.CXP_PackageType = framePckgType;
				package.CXP_MarksAndNumbers = vehicle;
			}

			var containers = item.Consignment.Header.CusExitContainers;
			foreach (var (qty, type, marks, contEquip) in listOfPackages.Values)
			{
				var pivot = item.CusExitConsignmentPackagePivots.AddNew();
				var package = pivot.Package;
				package.CXP_Quantity = qty;
				package.CXP_PackageType = type.Substring(0, CusExitConsignmentPackage.Schema.CXP_PackageTypeMaxLength);
				package.CXP_MarksAndNumbers = marks;
				if (!contEquip.IsEmpty)
				{
					pivot.CNP_CXN_Container = containers.FirstOrDefault(x => x.CXN_ContainerNumber == contEquip)?.PK ?? ZGuid.Empty;
				}
			}
		}

		void GetPackagesTuples(Customs.Business.BaseCusLinkPackage p, Dictionary<ZString, Tuple<ZInt, ZString, ZString, ZString>> listOfPackages)
		{
			var package = p.Package;
			var qtyToAdd = p.PackQty;
			var typeToAdd = package.CW_PackType;
			var marksToAdd = package.CW_MarksAndNos;

			var keyForPackages = typeToAdd + marksToAdd;
			if (listOfPackages.ContainsKey(keyForPackages))
			{
				var (qty, _, _, _) = listOfPackages[keyForPackages];
				qtyToAdd += qty;
				listOfPackages[keyForPackages] = new Tuple<ZInt, ZString, ZString, ZString>(qtyToAdd, typeToAdd, marksToAdd, ZString.Empty);
			}
			else
			{
				listOfPackages.Add(keyForPackages, new Tuple<ZInt, ZString, ZString, ZString>(qtyToAdd, typeToAdd, marksToAdd, package.CW_ContainerNoOrEquipmentNo));
			}
		}

		public IEnumerable<CusEntryHeader> GetEntriesWithMRNInAcceptedExitConsignments(IEnumerable<CusEntryHeader> acceptedEntries, ExitControlBase.Business.ICusExitReportCollection<CusExitReport> exitReports)
			=> acceptedEntries.Where(entry => exitReports.Any(report => (report.Consignment?.CXC_MovementReference ?? ZString.Empty) == entry.MovementReferenceNumber && report.IsSentOrAccepted));

		public IEnumerable<CusEntryHeader> GetEntriesWithMRNInNotAcceptedExitConsignments(IEnumerable<CusEntryHeader> acceptedEntries, CusExitHeader exitHeader)
		{
			var consignmentsWithoutReports = exitHeader.CusExitConsignments.Where(c => exitHeader.CusExitReports.All(r => r.CER_CXC_Consignment != c.PK));

			return acceptedEntries.Where(entry => exitHeader.CusExitReports.Any(report => (report.Consignment?.CXC_MovementReference ?? ZString.Empty) == entry.MovementReferenceNumber && !report.IsSentOrAccepted))
									.Union(acceptedEntries.Where(entry => consignmentsWithoutReports.Any(c => c.CXC_MovementReference == entry.MovementReferenceNumber)));
		}
	}
}
