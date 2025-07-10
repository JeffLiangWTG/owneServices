using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class PopulateExitControlHelperTest : TestCaseWithFactory
	{
		public void TestGenerateExitControl_AddContainers()
		{
			void AddContainerAndSeal(JobDeclaration dec, string contNumber, string seal1, string seal2)
			{
				var cont = dec.CusContainers.AddNew();
				cont.CO_ContainerNumber = "CONT-" + contNumber;
				cont.CO_Seal = seal1;
				cont.CO_SecondSeal = seal2;
				var seal = cont.AdditionalSeals.AddNew();
				seal.BK_SealNumber = "SEAL-" + contNumber;
			}
			var expectedHeaderReference = "Reference";

			var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: false);
			AddContainerAndSeal(declaration, "AH3", "S1", ZString.Empty);
			AddContainerAndSeal(declaration, "2", ZString.Empty, ZString.Empty);
			AddContainerAndSeal(declaration, "3", ZString.Empty, ZString.Empty);

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = expectedHeaderReference;
			exitHeader.CXH_GS_NKCustomsAgent = "AA";

			Factory.Save();

			var entryHeaderList = new List<Tuple<ZString, ZString>>();
			declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

			AssertEquals("Data is not copied to exitHeader yet", 0, exitHeader.CusExitContainers.Count);

			var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

			CombineAssertions(() =>
			{
				void AssertContainerCopied(CusExitContainer exitCont, string contNumber, int sealsCreated, params ZString[] seals)
				{
					AssertEquals("First container number", "CONT-" + contNumber, exitCont.CXN_ContainerNumber);
					AssertEquals("First container isEquipment", false, exitCont.CXN_IsEquipment);
					AssertEquals("First container seals", sealsCreated, exitCont.AllSealNumbers.Count);

					AssertArrayEqualsByElements("Container contains same seals", seals, exitCont.AllSealNumbers.Select(x => ((CusExitSeal)x).BK_SealNumber).ToArray());
				}

				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);

				AssertEquals("Data is copied to exitHeader", 3, exitHeader.CusExitContainers.Count);

				AssertContainerCopied((CusExitContainer)exitHeader.CusExitContainers[0], "AH3", 2, "S1", "SEAL-AH3");
				AssertContainerCopied((CusExitContainer)exitHeader.CusExitContainers[1], "2", 1, "SEAL-2");
				AssertContainerCopied((CusExitContainer)exitHeader.CusExitContainers[2], "3", 1, "SEAL-3");

				AddContainerAndSeal(declaration, "TEST", "S1", "S2");
				AddContainerAndSeal(declaration, "AH3", "S1", ZString.Empty);

				generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);
				AssertEquals("Data not duplicated is added to exitHeader", 4, exitHeader.CusExitContainers.Count);
				AssertContainerCopied((CusExitContainer)exitHeader.CusExitContainers[3], "TEST", 3, "S1", "S2", "SEAL-TEST");
			});
		}

		public void TestGenerateExitControl_AddEquipments()
		{
			void AddEquipmentAndSeal(JobDeclaration dec, string contNumber)
			{
				var equip = dec.Equipments.AddNew();
				equip.CEQ_IdentificationNumber = "EQUIP-" + contNumber;
				var seal = equip.Seals.AddNew();
				seal.BK_SealNumber = "SEAL-" + contNumber;
			}
			var expectedHeaderReference = "Reference";

			var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: false);
			AddEquipmentAndSeal(declaration, "AH3");
			AddEquipmentAndSeal(declaration, "2");
			AddEquipmentAndSeal(declaration, "3");

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = expectedHeaderReference;
			exitHeader.CXH_GS_NKCustomsAgent = "AA";

			Factory.Save();

			var entryHeaderList = new List<Tuple<ZString, ZString>>();
			declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

			AssertEquals("Data is not copied to exitHeader yet", 0, exitHeader.CusExitContainers.Count);

			var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

			CombineAssertions(() =>
			{
				void AssertEquipmentCopied(CusExitContainer exitCont, string contNumber)
				{
					AssertEquals("First equipment number", "EQUIP-" + contNumber, exitCont.CXN_ContainerNumber);
					AssertEquals("First equipment isEquipment", true, exitCont.CXN_IsEquipment);
					AssertEquals("First equipment seals", 1, exitCont.AllSealNumbers.Count);

					AssertEquals("Equipment contains same seals", "SEAL-" + contNumber, exitCont.AllSealNumbers[0].BK_SealNumber);
				}

				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);

				AssertEquals("Data is copied to exitHeader", 3, exitHeader.CusExitContainers.Count);
				AssertEquipmentCopied((CusExitContainer)exitHeader.CusExitContainers[0], "AH3");
				AssertEquipmentCopied((CusExitContainer)exitHeader.CusExitContainers[1], "2");
				AssertEquipmentCopied((CusExitContainer)exitHeader.CusExitContainers[2], "3");
			});
		}

		void SetUpForCusExitConsignmentItems(JobDeclaration declaration)
		{
			JobComInvoiceLine AddInvoiceLines(JobComInvoiceHeader invHeader, string tariff, string commercialRef, ZDecimal grossWeight, ZDecimal netWeight)
			{
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_Tariff = tariff;
				invLine.ZG_CommercialReference = commercialRef;
				invLine.JI_Weight = grossWeight;
				invLine.JI_WeightUQ = "KG";
				invLine.JI_NetWeight = netWeight;
				invLine.JI_NetWeightUQ = "KG";

				return invLine;
			}

			var invHeader1 = declaration.Invoices.AddNew();
			var invHeader2 = declaration.Invoices.AddNew();

			AddInvoiceLines(invHeader1, "20402040", "AH3", 2.5, 2.3);
			AddInvoiceLines(invHeader1, "20402040", "AH3", 2.5, 2.3);
			AddInvoiceLines(invHeader2, "30302020", "REF", 5.3, 4.5);
			AddInvoiceLines(invHeader2, "30302020", "REF", 5.2, 4.4);
		}

		public void TestGenerateExitControl_AddCusExitConsignmentItems()
		{
			var declaration = GetNewBasicJobDeclaration();
			SetUpForCusExitConsignmentItems(declaration);

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = "Reference";
			exitHeader.CXH_GS_NKCustomsAgent = "AA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Prereq: single header created", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Prereq: two entryLines created", 2, entryHeader.AllEntryLines.Count);
			var entryHeaderList = new List<Tuple<ZString, ZString>>
			{
				new Tuple<ZString, ZString>(entryHeader.MovementReferenceNumber, entryHeader.CH_BGMReference)
			};

			AssertEquals("Data is not copied to exitHeader yet", 0, exitHeader.CusExitContainers.Count);

			var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

			CombineAssertions(() =>
			{
				void AssertConsignmentCopied(string message, CusExitConsignment consignment, params (ZDecimal gross, ZDecimal net, string reference)[] expectedItemConsigments)
				{
					var consignmentItems = consignment.CusExitConsignmentItems;
					AssertEquals(message, expectedItemConsigments.Length, consignmentItems.Count);

					foreach (var (gross, net, reference) in expectedItemConsigments)
					{
						var keyForObject = gross.ToString() + net.ToString() + reference;
						AssertNotNull($"Contains object with expected values for {keyForObject}", consignmentItems.FirstOrDefault(i =>
						{
							return i.CCI_GrossMass == gross
								&& i.CCI_NetMass == net
								&& i.CCI_UniqueConsignmentReference == reference;
						}));
					}
				}

				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);

				AssertEquals("1 Consigment created for 1 entryHeader", 1, exitHeader.CusExitConsignments.Count);
				AssertConsignmentCopied("Expected ConsigmentItems created for entryLines", exitHeader.CusExitConsignments[0], (5, 4.6, "AH3"), (10.5, 8.9, "REF"));

				var invLine1 = declaration.Invoices[0].InvoiceLines[0];
				AssertEquals("Prereq: is invLine1 ref", "AH3", invLine1.ZG_CommercialReference);
				invLine1.ZG_CommercialReference = "AH2";
				invLine1.JI_Weight = 10.0;
				invLine1.JI_NetWeight = 12.2;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.ZG_UCC6Version = 1;
				generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);
				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
				AssertEquals("Same Consigment created before for 1 entryHeader", 1, exitHeader.CusExitConsignments.Count);
				AssertConsignmentCopied("Expected ConsigmentItems re-created for entryLines", exitHeader.CusExitConsignments[0], (2.5, 2.3, "AH3"), (10.0, 12.2, "AH2"), (10.5, 8.9, "REF"));
			});
		}

		void SetUpForCusExitConsignmentPackages(JobDeclaration declaration)
		{
			JobComInvoiceLine AddInvoiceLineWithVehicle(JobDeclaration dec, string vehicleVin, string brand, string model, string vehicleVin1, string brand1, string model1)
			{
				var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
				var vehicle = invLine.Vehicles.AddNew();
				vehicle.CVH_VehicleIdentificationNumber = vehicleVin;
				vehicle.CVH_BrandName = brand;
				vehicle.CVH_ModelName = model;
				if (!vehicleVin1.IsNullOrEmpty())
				{
					var vehicle1 = invLine.Vehicles.AddNew();
					vehicle1.CVH_VehicleIdentificationNumber = vehicleVin1;
					vehicle1.CVH_BrandName = brand1;
					vehicle1.CVH_ModelName = model1;
				}

				return invLine;
			}

			void AddPackagesToInvLine(JobDeclaration dec, JobComInvoiceLine invLine, params (string type, string marks, int packQty)[] packages)
			{
				var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
				foreach (var (type, marks, packQty) in packages)
				{
					var newPackage = dec.Packages.AddNew();
					var packagePivot = invLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
					packagePivot.Package = newPackage;
					packagePivot.IsLinked = true;
					packagePivot.PackQty = packQty;
					newPackage.CW_CR_HouseContainer = billPackingGroup.PK;
					newPackage.CW_PackType = type;
					newPackage.CW_MarksAndNos = type;
					newPackage.CW_PackQty = packQty + 1;
				}
			}

			var invLine1 = AddInvoiceLineWithVehicle(declaration, "VIN1", "B1", "M1", "VINX", "BX", "MX");
			AddPackagesToInvLine(declaration, invLine1, ("FR", "Mark", 1), ("Z", "Z", 2));

			AddInvoiceLineWithVehicle(declaration, "VIN2", "B2", "M2", "", "", "");

			var invLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AddPackagesToInvLine(declaration, invLine3, ("A3", "A3", 2), ("B", "B", 2), ("B", "B", 5), ("FR", "FR", 2));

			var invLine4 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AddPackagesToInvLine(declaration, invLine4, ("A4", "A4", 3));
		}

		public void TestGenerateExitControl_AddCusExitConsignmentPackages()
		{
			var declaration = GetNewBasicJobDeclaration();
			SetUpForCusExitConsignmentPackages(declaration);

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = "Reference";
			exitHeader.CXH_GS_NKCustomsAgent = "AA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Prereq: single header created", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Prereq: single entryLines created", 1, entryHeader.AllEntryLines.Count);

			var entryHeaderList = new List<Tuple<ZString, ZString>>
			{
				new Tuple<ZString, ZString>(entryHeader.MovementReferenceNumber, entryHeader.CH_BGMReference)
			};

			AssertEquals("Data is not copied to exitHeader yet", 0, exitHeader.CusExitContainers.Count);

			var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

			CombineAssertions(() =>
			{
				void AssertPackagesCopied(CusExitConsignment consignment, List<Tuple<string, string, int>> expectedPackages)
				{
					var firstItem = consignment.CusExitConsignmentItems[0];
					AssertEquals("Counts ", expectedPackages.Count, firstItem.CusExitConsignmentPackagePivots.Count);
					foreach (var (marks, pType, qty) in expectedPackages)
					{
						var keyForObject = qty.ToString() + pType + marks;
						AssertNotNull($"Contains object with expected values for {keyForObject}", firstItem.CusExitConsignmentPackagePivots.FirstOrDefault(p =>
						{
							var package = p.Package;
							return package.CXP_Quantity == qty
								&& package.CXP_PackageType == pType
								&& package.CXP_MarksAndNumbers == marks;
						}));
					}
				}

				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);

				AssertEquals("1 Consigment created for 1 entryHeader", 1, exitHeader.CusExitConsignments.Count);
				var expectedPckgs = new List<Tuple<string, string, int>>
				{
					new Tuple<string, string, int>("VIN1:B1:M1, VINX:BX:MX", "FR", 1),
					new Tuple<string, string, int>("VIN2:B2:M2", "FR", 1),
					new Tuple<string, string, int>("Z", "Z", 2),
					new Tuple<string, string, int>("A3", "A3", 2),
					new Tuple<string, string, int>("B", "B", 7),
					new Tuple<string, string, int>("FR", "FR", 2),
					new Tuple<string, string, int>("A4", "A4", 3),
				};
				AssertPackagesCopied(exitHeader.CusExitConsignments[0], expectedPckgs);

				var invLine3 = declaration.Invoices[2].InvoiceLines[0];
				var vehicle3 = invLine3.Vehicles.AddNew();
				vehicle3.CVH_VehicleIdentificationNumber = "VIN3";
				vehicle3.CVH_BrandName = "";
				vehicle3.CVH_ModelName = "M3";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.ZG_UCC6Version = 1;

				generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);
				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
				AssertEquals("1 Consigment previously created for 1 entryHeader", 1, exitHeader.CusExitConsignments.Count);

				expectedPckgs = new List<Tuple<string, string, int>>
					{
						new Tuple<string, string, int>("VIN1:B1:M1, VINX:BX:MX", "FR", 1),
						new Tuple<string, string, int>("VIN2:B2:M2", "FR", 1),
						new Tuple<string, string, int>("VIN3::M3", "FR", 1),
						new Tuple<string, string, int>("Z", "Z", 2),
						new Tuple<string, string, int>("A3", "A3", 2),
						new Tuple<string, string, int>("B", "B", 7),
						new Tuple<string, string, int>("A4", "A4", 3),
					};
				AssertPackagesCopied(exitHeader.CusExitConsignments[0], expectedPckgs);
			});
		}

		public void TestTestGenerateExitControl_AddCusExitConsignmentPackages_ContainerNo()
		{
			void AddPackage(JobDeclaration dec, JobComInvoiceLine invL, ZString containerNo, ZString packType)
			{
				var billPackingGroup = dec.Bills.AddNew().PackingGroups.AddNew();
				var newPackage = dec.Packages.AddNew();
				var packagePivot = invL.PackagesForInvoiceLinesForBindingOnly.AddNew();
				packagePivot.Package = newPackage;
				packagePivot.IsLinked = true;
				packagePivot.PackQty = 2;
				newPackage.CW_CR_HouseContainer = billPackingGroup.PK;
				newPackage.CW_PackType = packType;
				newPackage.CW_MarksAndNos = "Marks";
				newPackage.CW_PackQty = 2;
				newPackage.CW_ContainerNoOrEquipmentNo = containerNo;
			}
			var declaration = GetNewBasicJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT555";
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AddPackage(declaration, invLine, container.CO_ContainerNumber, "DD");
			AddPackage(declaration, invLine, container.CO_ContainerNumber, "AH");
			AddPackage(declaration, invLine, container.CO_ContainerNumber, "AH");

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = "Reference";
			exitHeader.CXH_GS_NKCustomsAgent = "AA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_UCC6Version = 1;

			var headerList = new List<Tuple<ZString, ZString>> { new Tuple<ZString, ZString>(entryHeader.MovementReferenceNumber, entryHeader.CH_BGMReference) };

			var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(headerList, exitHeader, declaration);

			CombineAssertions(() =>
			{
				var consignment = exitHeader.CusExitConsignments[0];
				var firstItem = consignment.CusExitConsignmentItems[0];
				AssertEquals(2, firstItem.CusExitConsignmentPackagePivots.Count);

				AssertEquals("Expected Container", consignment.Header.CusExitContainers[0].PK, firstItem.CusExitConsignmentPackagePivots[0].CNP_CXN_Container);
				AssertEquals("No Container for grouped packages", ZGuid.Empty, firstItem.CusExitConsignmentPackagePivots[1].CNP_CXN_Container);
			});
		}

		public void TestTrainingEntry()
		{
			var declaration = GetNewDeclarationForExitControlGeneration();
			declaration.ZG_IsTrainingDeclaration = true;

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;

			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Prereq: single entryLines created", 1, entryHeader.AllEntryLines.Count);

			var entryHeaderList = new List<Tuple<ZString, ZString>>
			{
				new Tuple<ZString, ZString>(entryHeader.MovementReferenceNumber, entryHeader.CH_BGMReference)
			};

			AssertEquals("Data is not copied to exitHeader yet", 0, exitHeader.CusExitContainers.Count);

			var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

			CombineAssertions(() =>
			{
				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
				AssertEquals("Expected TrainingEntry", true, exitHeader.TrainingEntry);

				declaration.ZG_IsTrainingDeclaration = false;
				new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);
				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
				AssertEquals("Expected TrainingEntry", false, exitHeader.TrainingEntry);
			});
		}

		public void TestGenerateExitControl_AcceptedHeader_ExistingExitHeaderWithoutExitConsignment()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: false);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = "AA";

				Factory.Save();

				var entryHeaderList = new List<Tuple<ZString, ZString>>();
				declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

				var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

				CombineAssertions(() =>
				{
					AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitConsignment }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
										ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGenerateExitControl_AcceptedHeader_ExistingExitHeaderAndConsignmentWithoutExitReport()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: false);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = "AA";

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;
				exitConsignment.CXC_LocalReference = "local ref";

				Factory.Save();

				var entryHeaderList = new List<Tuple<ZString, ZString>>();
				declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

				var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

				CombineAssertions(() =>
				{
					AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitConsignment }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
										ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGenerateExitControl_AcceptedHeader_ExistingExitHeaderWithExitReport()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: false);
				declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = "AA";

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;
				exitConsignment.CXC_LocalReference = "local ref";

				var exitReport = exitHeader.CusExitReports.AddNew();
				exitReport.CER_CXC_Consignment = exitConsignment.PK;
				exitReport.CER_OfficeOfExit = "AAA";
				exitReport.CER_TransportMode = "AIR";

				Factory.Save();

				var entryHeaderList = new List<Tuple<ZString, ZString>>();
				declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

				var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

				CombineAssertions(() =>
				{
					AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedReportsCount);
					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitConsignment }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
										ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGenerateExitControl_MultipleHeaders_ExistingExitHeaderWithoutExitConsignments()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = "AA";

				Factory.Save();

				var entryHeaderList = new List<Tuple<ZString, ZString>>();
				declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

				var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

				CombineAssertions(() =>
				{
					AssertEquals("GenerateExitControlFromEntries returns 3", 3, generatedReportsCount);
					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 3, new ZString[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment, ExpectedMRN3ForNewExitConsignment },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGenerateExitControl_MultipleHeaders_ExistingExitHeaderAndConsignmentsWithoutExitReports()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = "AA";

				var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;
				exitConsignment1.CXC_LocalReference = "local ref1";

				var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;
				exitConsignment2.CXC_LocalReference = "local ref2";

				var exitConsignment3 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment3.CXC_MovementReference = ExpectedMRN3ForNewExitConsignment;
				exitConsignment3.CXC_LocalReference = "local ref3";

				Factory.Save();

				var entryHeaderList = new List<Tuple<ZString, ZString>>();
				declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

				var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

				CombineAssertions(() =>
				{
					AssertEquals("GenerateExitControlFromEntries returns 3", 3, generatedReportsCount);
					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 3, new ZString[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment, ExpectedMRN3ForNewExitConsignment },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGenerateExitControl_MultipleHeaders_ExistingExitHeaderWithExitReports()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";
			var expectedCustomsOffice = "ES008888";
			var expectedInlandMOT = "AIR";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.JE_CustomsOffice = expectedCustomsOffice;
				declaration.JE_TransportModeInland = expectedInlandMOT;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = "AA";

				var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;
				exitConsignment1.CXC_LocalReference = "local ref1";

				var exitReport1 = exitHeader.CusExitReports.AddNew();
				exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
				exitReport1.CER_OfficeOfExit = "AAA";
				exitReport1.CER_TransportMode = "AIR";

				var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;
				exitConsignment2.CXC_LocalReference = "local ref2";

				var exitReport2 = exitHeader.CusExitReports.AddNew();
				exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
				exitReport2.CER_OfficeOfExit = "AAA";
				exitReport2.CER_TransportMode = "AIR";

				var exitConsignment3 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment3.CXC_MovementReference = ExpectedMRN3ForNewExitConsignment;
				exitConsignment3.CXC_LocalReference = "local ref3";

				var exitReport3 = exitHeader.CusExitReports.AddNew();
				exitReport3.CER_CXC_Consignment = exitConsignment3.PK;
				exitReport3.CER_OfficeOfExit = "AAA";
				exitReport3.CER_TransportMode = "AIR";

				Factory.Save();

				var entryHeaderList = new List<Tuple<ZString, ZString>>();
				declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ForEach(entry => entryHeaderList.Add(new Tuple<ZString, ZString>(entry.MovementReferenceNumber, entry.CH_BGMReference)));

				var generatedReportsCount = new PopulateExitControlHelper().GenerateExitControlFromEntries(entryHeaderList, exitHeader, declaration);

				CombineAssertions(() =>
				{
					AssertEquals("GenerateExitControlFromEntries returns 3", 3, generatedReportsCount);
					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 3, new ZString[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment, ExpectedMRN3ForNewExitConsignment },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK,
									expectedHeaderReference: expectedHeaderReference, expectedCustomsOffice: expectedCustomsOffice, expectedInlandMOT: expectedInlandMOT);
				});
			}
		}

		public void TestGetEntriesWithMRNInAcceptedExitConsignments()
		{
			var mrn4 = "refNum4";
			var mrn5 = "refNum5";

			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.MovementReferenceNumber = ExpectedMRN1ForNewExitConsignment;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.MovementReferenceNumber = ExpectedMRN2ForNewExitConsignment;
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.MovementReferenceNumber = ExpectedMRN3ForNewExitConsignment;
			var entry4 = declaration.CustomsEntryHeaders.AddNew();
			entry4.MovementReferenceNumber = mrn4;
			var entry5 = declaration.CustomsEntryHeaders.AddNew();
			entry5.MovementReferenceNumber = mrn5;

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

			var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;

			var exitReport1 = exitHeader.CusExitReports.AddNew();
			exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
			exitReport1.CER_MessageStatus = "SNT";

			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;

			var exitReport2 = exitHeader.CusExitReports.AddNew();
			exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
			exitReport2.CER_Status = ZString.Empty;

			var exitConsignment3 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment3.CXC_MovementReference = ExpectedMRN3ForNewExitConsignment;

			var exitReport3 = exitHeader.CusExitReports.AddNew();
			exitReport3.CER_CXC_Consignment = exitConsignment3.PK;
			exitReport3.CER_Status = "EXR";

			var exitConsignment4 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment4.CXC_MovementReference = mrn4;

			var exitConsignment5 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment5.CXC_MovementReference = mrn5;

			var exitReport5 = exitHeader.CusExitReports.AddNew();
			exitReport5.CER_CXC_Consignment = exitConsignment5.PK;
			exitReport5.CER_Status = "COX";

			AssertContainsExactElementsInAnyOrder("Should have only entries 1, 3 and 5", new[] { entry1, entry3, entry5 }, new PopulateExitControlHelper().GetEntriesWithMRNInAcceptedExitConsignments(declaration.CustomsEntryHeaders, exitHeader.CusExitReports));
		}

		public void TestGetEntriesWithMRNInNotAcceptedExitConsignments()
		{
			var mrn4 = "refNum4";
			var mrn5 = "refNum5";

			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.MovementReferenceNumber = ExpectedMRN1ForNewExitConsignment;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.MovementReferenceNumber = ExpectedMRN2ForNewExitConsignment;
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.MovementReferenceNumber = ExpectedMRN3ForNewExitConsignment;
			var entry4 = declaration.CustomsEntryHeaders.AddNew();
			entry4.MovementReferenceNumber = mrn4;
			var entry5 = declaration.CustomsEntryHeaders.AddNew();
			entry5.MovementReferenceNumber = mrn5;

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

			var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;

			var exitReport1 = exitHeader.CusExitReports.AddNew();
			exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
			exitReport1.CER_MessageStatus = "SNT";

			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;

			var exitReport2 = exitHeader.CusExitReports.AddNew();
			exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
			exitReport2.CER_Status = ZString.Empty;

			var exitConsignment3 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment3.CXC_MovementReference = ExpectedMRN3ForNewExitConsignment;

			var exitReport3 = exitHeader.CusExitReports.AddNew();
			exitReport3.CER_CXC_Consignment = exitConsignment3.PK;
			exitReport3.CER_Status = "CLP";

			var exitConsignment4 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment4.CXC_MovementReference = mrn4;

			var exitConsignment5 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment5.CXC_MovementReference = mrn5;

			var exitReport5 = exitHeader.CusExitReports.AddNew();
			exitReport5.CER_CXC_Consignment = exitConsignment5.PK;
			exitReport5.CER_Status = "COX";

			AssertContainsExactElementsInAnyOrder("Should have only entries 2, 3 and 4", new[] { entry2, entry3, entry4 }, new PopulateExitControlHelper().GetEntriesWithMRNInNotAcceptedExitConsignments(declaration.CustomsEntryHeaders, exitHeader));
		}

		public void TestGetOrCreateExitHeader_NewExitHeader()
		{
			var expectedBrokerCode = "XZX";
			var expectedCertificate = "TESTCERT1";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;
			var wrapper = GlbStaffWrapper.Get(staffCurrentUser);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = expectedCertificate;
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				Factory.Save();

				(var exitHeadersReturned, var mrnsExistingButNotAssociatedList, var mrnsExistingButNotAssociatedErrorMessageList) = new PopulateExitControlHelper().GetOrCreateExitHeader(declaration, declaration.CustomsEntryHeaders);

				CombineAssertions(() =>
				{
					AssertEquals("Returned exitHeadersList has 1 header", 1, exitHeadersReturned.Count());
					AssertEquals("Returned mrnsExistingButNotAssociatedList is empty", false, mrnsExistingButNotAssociatedList.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedErrorMessageList is empty", false, mrnsExistingButNotAssociatedErrorMessageList.Any());

					var exitHeader = GetExitHeaderForDeclaration(declaration);
					AssertExitHeader("New ExitHeader", exitHeader, 0, Array.Empty<ZString>(), Array.Empty<ZString>(), expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedCertificate: expectedCertificate);
				});
			}
		}

		public void TestGetOrCreateExitHeader_ExistingExitHeader_AssociatedToDeclaration()
		{
			var expectedBrokerCode = "XZX";
			var expectedCertificate = "TESTCERT1";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;
			var wrapper = GlbStaffWrapper.Get(staffCurrentUser);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = expectedCertificate;
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;

				Factory.Save();

				(var exitHeadersReturned, var mrnsExistingButNotAssociatedList, var mrnsExistingButNotAssociatedErrorMessageList) = new PopulateExitControlHelper().GetOrCreateExitHeader(declaration, declaration.CustomsEntryHeaders);

				CombineAssertions(() =>
				{
					AssertEquals("Returned exitHeadersList has 1 header", 1, exitHeadersReturned.Count());
					AssertEquals("Returned mrnsExistingButNotAssociatedList is empty", false, mrnsExistingButNotAssociatedList.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedErrorMessageList is empty", false, mrnsExistingButNotAssociatedErrorMessageList.Any());

					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 0, Array.Empty<ZString>(), Array.Empty<ZString>(),
										expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedCertificate: expectedCertificate, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGetOrCreateExitHeader_ExistingExitHeaderWithoutConsignment_NotAssociatedToDeclaration()
		{
			var expectedExistingBrokerCode = "AA";
			var expectedDeclarationBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedDeclarationBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_JobReference = expectedHeaderReference;
				exitHeader.CXH_GS_NKCustomsAgent = expectedExistingBrokerCode;

				Factory.Save();

				(var exitHeadersReturned, var mrnsExistingButNotAssociatedList, var mrnsExistingButNotAssociatedErrorMessageList) = new PopulateExitControlHelper().GetOrCreateExitHeader(declaration, declaration.CustomsEntryHeaders);

				CombineAssertions(() =>
				{
					AssertEquals("Returned exitHeadersList has 1 header", 1, exitHeadersReturned.Count());
					AssertEquals("Returned mrnsExistingButNotAssociatedList is empty", false, mrnsExistingButNotAssociatedList.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedErrorMessageList is empty", false, mrnsExistingButNotAssociatedErrorMessageList.Any());

					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertNotEquals("There is a new exitHeader created since there was already one but is was not associated to the declaration and it had no MRNs", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("New exit header with new data", exitHeaderAfterGeneration, 0, Array.Empty<ZString>(), Array.Empty<ZString>(),
										expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedDeclarationBrokerCode);

					AssertExitHeader("Existing exit header has no new data", exitHeader, 0, Array.Empty<ZString>(), Array.Empty<ZString>(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty, expectedBroker: expectedExistingBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestGetOrCreateExitHeader_ExistingExitHeaderWithConsignment_NotAssociatedToDeclaration()
		{
			var expectedBrokerCode = "AA";
			var expectedHeaderReference1 = "ExitRef1";
			var expectedHeaderReference2 = "ExitRef2";
			var expectedDeclarationReference2 = "DecRef2";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = "XZX";
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader1 = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader1.CXH_JobReference = expectedHeaderReference1;
				exitHeader1.CXH_GS_NKCustomsAgent = expectedBrokerCode;

				var exitConsignment1 = exitHeader1.CusExitConsignments.AddNew();
				exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;

				var exitReport1 = exitHeader1.CusExitReports.AddNew();
				exitReport1.CER_CXC_Consignment = exitConsignment1.PK;

				var exitConsignment2 = exitHeader1.CusExitConsignments.AddNew();
				exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;

				var exitReport2 = exitHeader1.CusExitReports.AddNew();
				exitReport2.CER_CXC_Consignment = exitConsignment2.PK;

				var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
				declaration2.JE_DeclarationReference = expectedDeclarationReference2;
				var exitHeader2 = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader2.CXH_ParentID = declaration2.PK;
				exitHeader2.CXH_ParentTableCode = declaration2.TablePrefix;
				exitHeader2.CXH_JobReference = expectedHeaderReference2;
				exitHeader2.CXH_GS_NKCustomsAgent = expectedBrokerCode;

				var exitConsignment3 = exitHeader2.CusExitConsignments.AddNew();
				exitConsignment3.CXC_MovementReference = ExpectedMRN3ForNewExitConsignment;

				var exitReport3 = exitHeader2.CusExitReports.AddNew();
				exitReport3.CER_CXC_Consignment = exitConsignment3.PK;

				Factory.Save();

				(var exitHeadersReturned, var mrnsExistingButNotAssociatedList, var mrnsExistingButNotAssociatedErrorMessageList) = new PopulateExitControlHelper().GetOrCreateExitHeader(declaration, declaration.CustomsEntryHeaders);

				CombineAssertions(() =>
				{
					AssertEquals("Returned exitHeadersList has 0 headers", false, exitHeadersReturned.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedList is not empty", true, mrnsExistingButNotAssociatedList.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedErrorMessageList is not empty", true, mrnsExistingButNotAssociatedErrorMessageList.Any());

					AssertContainsExactElementsInAnyOrder("mrnsExistingButNotAssociatedList contains correct mrns", new[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment, ExpectedMRN3ForNewExitConsignment }, mrnsExistingButNotAssociatedList);
					AssertContainsExactElementsInAnyOrder("mrnsExistingButNotAssociatedErrorMessageList contains correct mrn and exitheader ref or declaration ref when it is the parent in the message",
						new[] { "A movement for MRN " + ExpectedMRN1ForNewExitConsignment + " already exists in Exit Control " + expectedHeaderReference1,
								"A movement for MRN " + ExpectedMRN2ForNewExitConsignment + " already exists in Exit Control " + expectedHeaderReference1,
								"A movement for MRN " + ExpectedMRN3ForNewExitConsignment + " already exists in Job Number " + expectedDeclarationReference2 },
						mrnsExistingButNotAssociatedErrorMessageList);

					GetExitHeaderForDeclaration(declaration, false);

					AssertExitHeader("Existing exit header 1 has no new data", exitHeader1, 2, new ZString[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment },
									new ZString[] { ZString.Empty, ZString.Empty }, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty,
									expectedParentTableCode: ZString.Empty, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference1, expectedCustomsOffice: ZString.Empty, expectedInlandMOT: ZString.Empty);

					AssertExitHeader("Existing exit header 2 has no new data", exitHeader2, 1, new ZString[] { ExpectedMRN3ForNewExitConsignment },
									new ZString[] { ZString.Empty }, ZGuid.Empty, ZGuid.Empty, declaration2.PK,
									expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference2, expectedCustomsOffice: ZString.Empty, expectedInlandMOT: ZString.Empty);
				});
			}
		}

		public void TestGetOrCreateExitHeader_ExistingExitHeaderWithConsignment_OnlyOneAssociatedToDeclaration()
		{
			var expectedExistingBrokerCode = "AA";
			var expectedDeclarationBrokerCode = "XZX";
			var expectedHeaderReference1 = "ExitRef1";
			var expectedHeaderReference2 = "ExitRef2";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedDeclarationBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader1 = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader1.CXH_JobReference = expectedHeaderReference1;
				exitHeader1.CXH_GS_NKCustomsAgent = expectedExistingBrokerCode;

				var exitConsignment1 = exitHeader1.CusExitConsignments.AddNew();
				exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;

				var exitReport1 = exitHeader1.CusExitReports.AddNew();
				exitReport1.CER_CXC_Consignment = exitConsignment1.PK;

				var exitConsignment2 = exitHeader1.CusExitConsignments.AddNew();
				exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;

				var exitReport2 = exitHeader1.CusExitReports.AddNew();
				exitReport2.CER_CXC_Consignment = exitConsignment2.PK;

				var exitHeader2 = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader2.CXH_ParentID = declaration.PK;
				exitHeader2.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader2.CXH_JobReference = expectedHeaderReference2;

				Factory.Save();

				(var exitHeadersReturned, var mrnsExistingButNotAssociatedList, var mrnsExistingButNotAssociatedErrorMessageList) = new PopulateExitControlHelper().GetOrCreateExitHeader(declaration, declaration.CustomsEntryHeaders);

				CombineAssertions(() =>
				{
					AssertEquals("Returned exitHeadersList has 1 header", 1, exitHeadersReturned.Count());
					AssertEquals("Returned mrnsExistingButNotAssociatedList is not empty", true, mrnsExistingButNotAssociatedList.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedErrorMessageList is not empty", true, mrnsExistingButNotAssociatedErrorMessageList.Any());

					AssertContainsExactElementsInAnyOrder("mrnsExistingButNotAssociatedList contains correct mrns", new[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment }, mrnsExistingButNotAssociatedList);
					AssertContainsExactElementsInAnyOrder("mrnsExistingButNotAssociatedErrorMessageList contains correct mrn and exitheader ref or declaration ref when it is the parent in the message",
					new[] { "A movement for MRN " + ExpectedMRN1ForNewExitConsignment + " already exists in Exit Control " + expectedHeaderReference1,
								"A movement for MRN " + ExpectedMRN2ForNewExitConsignment + " already exists in Exit Control " + expectedHeaderReference1 },
						mrnsExistingButNotAssociatedErrorMessageList);

					var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
					AssertNotEquals("There is no new exitHeader created but is is not the same as the first exitHeader since it was not associated and it had mrns", exitHeader1, exitHeaderAfterGeneration);
					AssertEquals("There is no new exitHeader created since there was already one  associated to the declaration", exitHeader2, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header 1 has no new data", exitHeader1, 2, new ZString[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment },
									new ZString[] { ZString.Empty, ZString.Empty }, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty,
									expectedParentTableCode: ZString.Empty, expectedBroker: expectedExistingBrokerCode, expectedHeaderReference: expectedHeaderReference1, expectedCustomsOffice: ZString.Empty, expectedInlandMOT: ZString.Empty);

					AssertExitHeader("Existing exit header 2 has new data", exitHeaderAfterGeneration, 0, Array.Empty<ZString>(),
									Array.Empty<ZString>(), expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK,
									expectedBroker: expectedDeclarationBrokerCode, expectedHeaderReference: expectedHeaderReference2, expectedCustomsOffice: ZString.Empty, expectedInlandMOT: ZString.Empty);
				});
			}
		}

		public void TestGetOrCreateExitHeader_ExistingExitHeaderWithConsignment_NotAssociatedToDeclarationExceptOneMRN()
		{
			var expectedExistingBrokerCode = "AA";
			var expectedDeclarationBrokerCode = "XZX";
			var expectedHeaderReference1 = "ExitRef1";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedDeclarationBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = GetNewDeclarationForExitControlGeneration(createSecondEntry: true, createThirdEntry: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader1 = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader1.CXH_JobReference = expectedHeaderReference1;
				exitHeader1.CXH_GS_NKCustomsAgent = expectedExistingBrokerCode;

				var exitConsignment1 = exitHeader1.CusExitConsignments.AddNew();
				exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitConsignment;

				var exitReport1 = exitHeader1.CusExitReports.AddNew();
				exitReport1.CER_CXC_Consignment = exitConsignment1.PK;

				var exitConsignment2 = exitHeader1.CusExitConsignments.AddNew();
				exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitConsignment;

				var exitReport2 = exitHeader1.CusExitReports.AddNew();
				exitReport2.CER_CXC_Consignment = exitConsignment2.PK;

				Factory.Save();

				(var exitHeadersReturned, var mrnsExistingButNotAssociatedList, var mrnsExistingButNotAssociatedErrorMessageList) = new PopulateExitControlHelper().GetOrCreateExitHeader(declaration, declaration.CustomsEntryHeaders);

				CombineAssertions(() =>
				{
					AssertEquals("Returned exitHeadersList has 1 header", 1, exitHeadersReturned.Count());
					AssertEquals("Returned mrnsExistingButNotAssociatedList is not empty", true, mrnsExistingButNotAssociatedList.Any());
					AssertEquals("Returned mrnsExistingButNotAssociatedErrorMessageList is not empty", true, mrnsExistingButNotAssociatedErrorMessageList.Any());

					AssertContainsExactElementsInAnyOrder("mrnsExistingButNotAssociatedList contains correct mrns", new[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment }, mrnsExistingButNotAssociatedList);
					AssertContainsExactElementsInAnyOrder("mrnsExistingButNotAssociatedErrorMessageList contains correct mrn and exitheader ref or declaration ref when it is the parent in the message",
						new[] { "A movement for MRN " + ExpectedMRN1ForNewExitConsignment + " already exists in Exit Control " + expectedHeaderReference1,
								"A movement for MRN " + ExpectedMRN2ForNewExitConsignment + " already exists in Exit Control " + expectedHeaderReference1, },
						mrnsExistingButNotAssociatedErrorMessageList);

					var newExitHeader = GetExitHeaderForDeclaration(declaration);

					AssertExitHeader("Existing exit header 1 has no new data", exitHeader1, 2, new ZString[] { ExpectedMRN1ForNewExitConsignment, ExpectedMRN2ForNewExitConsignment },
									new ZString[] { ZString.Empty, ZString.Empty }, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty,
									expectedParentTableCode: ZString.Empty, expectedBroker: expectedExistingBrokerCode, expectedHeaderReference: expectedHeaderReference1, expectedCustomsOffice: ZString.Empty, expectedInlandMOT: ZString.Empty);

					AssertExitHeader("New ExitHeader", newExitHeader, 0, Array.Empty<ZString>(), Array.Empty<ZString>(), expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedDeclarationBrokerCode);
				});
			}
		}

		public void TestGetOrCreateExitHeader_ShipmentOrJobDeclaration()
		{
			var declarationNoShipment = GetNewJobDeclaration();
			(var exitHeadersReturnedNoShipment, var _, var _) = new PopulateExitControlHelper().GetOrCreateExitHeader(declarationNoShipment, declarationNoShipment.CustomsEntryHeaders);

			var declarationWithShipment = GetNewJobDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			declarationWithShipment.JE_JS = shipment.PK;
			(var exitHeadersReturnedWithShipment, var _, var _) = new PopulateExitControlHelper().GetOrCreateExitHeader(declarationWithShipment, declarationWithShipment.CustomsEntryHeaders);

			CombineAssertions(() =>
			{
				AssertEquals("ExitHeader from a declaration with no shipment should has declaration.PK as Parent ID", declarationNoShipment.PK, exitHeadersReturnedNoShipment.FirstOrDefault().CXH_ParentID);
				AssertEquals("ExitHeader from a declaration with no shipment should has declaration.TablePrefix as Parent Table Code", declarationNoShipment.TablePrefix, exitHeadersReturnedNoShipment.FirstOrDefault().CXH_ParentTableCode);
				(var foundExitHeadersReturnedNoShipment, var _, var _) = new PopulateExitControlHelper().GetOrCreateExitHeader(declarationNoShipment, declarationNoShipment.CustomsEntryHeaders);
				AssertEquals("ExitHeader from a declaration with no shipment should be the same as generated before", exitHeadersReturnedNoShipment.FirstOrDefault().PK, foundExitHeadersReturnedNoShipment.FirstOrDefault().PK);

				AssertEquals("ExitHeader from a declaration with shipment should has shipment.PK as Parent ID", shipment.PK, exitHeadersReturnedWithShipment.FirstOrDefault().CXH_ParentID);
				AssertEquals("ExitHeader from a declaration with shipment should has shipment.TablePrefix as Parent Table Code", shipment.TablePrefix, exitHeadersReturnedWithShipment.FirstOrDefault().CXH_ParentTableCode);
				(var foundExitHeadersReturnedWithShipment, var _, var _) = new PopulateExitControlHelper().GetOrCreateExitHeader(declarationWithShipment, declarationWithShipment.CustomsEntryHeaders);
				AssertEquals("ExitHeader from a declaration with shipment should be the same as generated before", exitHeadersReturnedWithShipment.FirstOrDefault().PK, foundExitHeadersReturnedWithShipment.FirstOrDefault().PK);
			});
		}

		void AssertExitHeader(string message, CusExitHeader header, int expectedConsignmentReportCount, ZString[] expectedMRNCodes, ZString[] expectedLRNCodes, ZGuid expectedExporterID, ZGuid expectedCarrierID, ZGuid expectedParentID, string expectedParentTableCode = "JE", string expectedBroker = ExpectedBrokerCodeExitHeader,
								string expectedCertificate = "", string expectedHeaderReference = ExpectedReferenceForNewExitHeader, string expectedCustomsOffice = ExpectedCustomsOfficeForNewExitReport, string expectedInlandMOT = ExpectedInlandMOTForNewExitReport)
		{
			AssertEquals(message + " Header.CXH_GS_NKCustomsAgent", expectedBroker, header.CXH_GS_NKCustomsAgent);
			AssertEquals(message + " Header.CXH_CustomsProfile", expectedCertificate, header.CXH_CustomsProfile);
			AssertEquals(message + " Header.CXH_OH_Exporter", expectedExporterID, header.CXH_OH_Exporter);
			AssertEquals(message + " Header.CXH_OA_Carrier", expectedCarrierID, header.CXH_OA_Carrier);
			AssertEquals(message + " Header.CXH_ParentID", expectedParentID, header.CXH_ParentID);
			AssertEquals(message + " Header.CXH_ParentTableCode", expectedParentTableCode, header.CXH_ParentTableCode);
			AssertEquals(message + " Header.CXH_JobReference", expectedHeaderReference, header.CXH_JobReference);

			AssertExitConsignments(expectedConsignmentReportCount, header.CusExitConsignments, expectedMRNCodes, expectedLRNCodes);

			AssertExitReports(expectedConsignmentReportCount, header.CusExitReports, expectedMRNCodes, expectedCustomsOffice, expectedInlandMOT);
		}

		void AssertExitConsignments(int expectedConsignmentCount, ICusExitConsignmentCollection<CusExitConsignment> exitConsignments, ZString[] expectedMRNCodes, ZString[] expectedLRNCodes)
		{
			AssertEquals("ExitConsignments count is correct", expectedConsignmentCount, exitConsignments.Count);
			AssertContainsExactElementsInAnyOrder("ExitConsignments mrn codes are correct", expectedMRNCodes, exitConsignments.Select(x => x.CXC_MovementReference).ToArray());
			AssertContainsExactElementsInAnyOrder("ExitConsignments lrn codes are correct", expectedLRNCodes, exitConsignments.Select(x => x.CXC_LocalReference).ToArray());
		}

		void AssertExitReports(int expectedReportCount, ICusExitReportCollection<CusExitReport> exitReports, ZString[] expectedMRNCodes, string expectedCustomsOffice = ExpectedCustomsOfficeForNewExitReport, string expectedInlandMOT = ExpectedInlandMOTForNewExitReport)
		{
			AssertEquals("ExitReports count is correct", expectedReportCount, exitReports.Count);
			AssertContainsExactElementsInAnyOrder("ExitReports mrn codes are correct", expectedMRNCodes, exitReports.Select(x => x.Consignment.CXC_MovementReference).ToArray());
			AssertEquals("ExitReports Customs Office codes are correct", false, exitReports.Any(x => x.CER_OfficeOfExit != expectedCustomsOffice));
			AssertEquals("ExitReports Transport Mode codes are correct", false, exitReports.Any(x => x.CER_TransportMode != expectedInlandMOT));
		}

		JobDeclaration GetNewDeclarationForExitControlGeneration(bool createSecondEntry = false, bool createThirdEntry = false, bool thirdEntryShouldBeAccepted = false)
		{
			var declaration = GetNewJobDeclaration(createSecondEntry: createSecondEntry, createThirdEntry: createThirdEntry);
			declaration.JE_DeclarationReference = ExpectedReferenceForNewExitHeader;

			var entryHeader1 = declaration.CustomsEntryHeaders[0];
			entryHeader1.CH_EntryStatus = "EXR";
			entryHeader1.MovementReferenceNumber = ExpectedMRN1ForNewExitConsignment;
			entryHeader1.CH_BGMReference = ExpectedLRN1ForNewExitConsignment;

			if (createSecondEntry)
			{
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.CH_EntryStatus = "COX";
				entryHeader2.MovementReferenceNumber = ExpectedMRN2ForNewExitConsignment;
				entryHeader2.CH_BGMReference = ExpectedLRN2ForNewExitConsignment;
			}

			if (createThirdEntry)
			{
				var entryHeader3 = declaration.CustomsEntryHeaders[2];
				entryHeader3.CH_EntryStatus = thirdEntryShouldBeAccepted ? "CLP" : "AAA";
				entryHeader3.MovementReferenceNumber = ExpectedMRN3ForNewExitConsignment;
				entryHeader3.CH_BGMReference = ExpectedLRN3ForNewExitConsignment;
			}

			return declaration;
		}

		JobDeclaration GetNewBasicJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}

		JobDeclaration GetNewJobDeclaration(bool createSecondEntry = false, bool createThirdEntry = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader1 = declaration.Invoices.AddNew();

			GetNewInvoiceLine(declaration, invoiceHeader1);

			if (createSecondEntry)
			{
				var invoiceHeader2 = declaration.Invoices.AddNew();
				GetNewInvoiceLine(declaration, invoiceHeader2);
			}

			if (createThirdEntry)
			{
				var invoiceHeader3 = declaration.Invoices.AddNew();
				GetNewInvoiceLine(declaration, invoiceHeader3);
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			return declaration;
		}

		JobComInvoiceLine GetNewInvoiceLine(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			return invoiceLine;
		}

		CusExitHeader GetExitHeaderForDeclaration(JobDeclaration declaration, bool exitHeaderShouldExist = true)
		{
			var query = new ZQuery(CusExitHeaderSchema.CXH_ParentID, declaration.PK);
			var exitHeaders = Factory.Load<CusExitHeader>(query);

			if (exitHeaderShouldExist)
			{
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);
				return exitHeaders[0];
			}
			else
			{
				AssertEquals("No exitHeaders associated to the declaration", 0, exitHeaders.Length);
				return null;
			}
		}

		const string ExpectedMRN1ForNewExitConsignment = "refNum1";
		const string ExpectedMRN2ForNewExitConsignment = "refNum2";
		const string ExpectedMRN3ForNewExitConsignment = "refNum3";
		const string ExpectedLRN1ForNewExitConsignment = "ES00001";
		const string ExpectedLRN2ForNewExitConsignment = "ES00002";
		const string ExpectedLRN3ForNewExitConsignment = "ES00003";
		const string ExpectedReferenceForNewExitHeader = "JD0001";
		const string ExpectedBrokerCodeExitHeader = "AA";
		const string ExpectedCustomsOfficeForNewExitReport = "ES009999";
		const string ExpectedInlandMOTForNewExitReport = "SEA";
	}
}
