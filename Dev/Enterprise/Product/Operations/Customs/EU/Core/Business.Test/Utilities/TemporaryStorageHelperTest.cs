using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs.TemporaryStorage;
using EUInterfaces = Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.Utilities.Testing
{
	public class TemporaryStorageHelperTest : TestCaseWithFactory
	{
		public void TestIsTemporaryStorageRegisterEnabled()
		{
			var country = Core.Constants.CountryCodes.Latvia;
			var registryRegisterEnabled = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabled;
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				CombineAssertions(() =>
				{
					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = false, TemporaryStorageRegister is not enabled", false, IsTemporaryStorageRegisterEnabled(country));
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = false, TemporaryStorageRegister is enabled", true, IsTemporaryStorageRegisterEnabled(country));
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = true, TemporaryStorageRegister is enabled", true, IsTemporaryStorageRegisterEnabled(country));
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true, TemporaryStorageRegister is enabled", true, IsTemporaryStorageRegisterEnabled(country));
					}
				});
			}
		}

		public void TestIsLocationManagedInPremises()
		{
			var expectedType = "ADT";
			var expectedLocation = "ES009999AH3";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AH3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Code = "AH3";
			premises1.SRP_Description = "Desc";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			premises1.SRP_Type = expectedType;
			premises1.SRP_CustomsLocation = "location";

			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Code = "AZM";
			premises2.SRP_Description = "Desc2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			premises2.SRP_Type = "AAA";
			premises2.SRP_CustomsLocation = expectedLocation;

			CombineAssertions(() =>
			{
				AssertEquals("Returns true when Premises exist in database and at least one has the expected location (type is ignored)", true, IsLocationManagedInPremises(Factory, expectedLocation));

				AssertEquals("Returns false when Premises exist in database but type/location is not the expected one", false, IsLocationManagedInPremises(Factory, expectedLocation, expectedType));

				premises1.SRP_CustomsLocation = expectedLocation;
				AssertEquals("Returns true when Premises exist in database and at least one has the expected type and location", true, IsLocationManagedInPremises(Factory, expectedLocation, expectedType));

				premises1.SRP_IsActive = false;
				AssertEquals("Returns false when Premises is deactivated", false, IsLocationManagedInPremises(Factory, expectedLocation, expectedType));

				premises1.SRP_IsActive = true;
				premises1.Delete();
				premises2.Delete();
				AssertEquals("Returns false when no Premises exist in database", false, IsLocationManagedInPremises(Factory, expectedLocation, expectedType));
			});
		}

		public void TestGetManagedPremises()
		{
			var expectedType = "ADT";
			var expectedLocation = "ES009999AH3";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AH3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Code = "AH3";
			premises1.SRP_Description = "Desc";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			premises1.SRP_Type = expectedType;
			premises1.SRP_CustomsLocation = "location";
			premises1.SRP_IsActive = true;

			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Code = "AZM";
			premises2.SRP_Description = "Desc2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			premises2.SRP_Type = "AAA";
			premises2.SRP_CustomsLocation = expectedLocation;
			premises2.SRP_IsActive = true;

			CombineAssertions(() =>
			{
				AssertNull("Returns null when Premises exist in database but type/location is not the expected one", GetManagedPremises(Factory, expectedType, expectedLocation));

				premises1.SRP_CustomsLocation = expectedLocation;
				AssertEquals("Returns the first premises when Premises exist in database and at least one has the expected type and location", premises1, GetManagedPremises(Factory, expectedType, expectedLocation));

				premises1.SRP_IsActive = false;
				AssertNull("Returns null when Premises is deactivated", GetManagedPremises(Factory, expectedType, expectedLocation));

				premises1.SRP_IsActive = true;
				premises1.Delete();
				premises2.Delete();
				AssertNull("Returns null when no Premises exist in database", GetManagedPremises(Factory, expectedType, expectedLocation));
			});
		}

		public void TestCancelPendingRegLineTransactions()
		{
			var expectedInternalRefNum = "ES009999AH3";
			var expectedInternalRefType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;

			var transaction1 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction1.SRT_InternalReferenceNumber = expectedInternalRefNum;
			transaction1.SRT_InternalReferenceType = expectedInternalRefType;

			var transaction2 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction2.SRT_InternalReferenceNumber = expectedInternalRefNum;
			transaction2.SRT_InternalReferenceType = expectedInternalRefType;

			var transaction3 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
			transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction3.SRT_InternalReferenceNumber = expectedInternalRefNum;
			transaction3.SRT_InternalReferenceType = expectedInternalRefType;

			var transaction4 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction4.SRT_InternalReferenceNumber = expectedInternalRefNum;
			transaction4.SRT_InternalReferenceType = expectedInternalRefType;

			var transaction5 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
			transaction5.SRT_InternalReferenceNumber = expectedInternalRefNum;
			transaction5.SRT_InternalReferenceType = expectedInternalRefType;

			var transaction6 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction6.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction6.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction6.SRT_InternalReferenceNumber = "reference";
			transaction6.SRT_InternalReferenceType = expectedInternalRefType;

			var transaction7 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction7.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction7.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction7.SRT_InternalReferenceNumber = expectedInternalRefNum;
			transaction7.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;

			CancelPendingRegLineTransactions(Factory, expectedInternalRefNum, expectedInternalRefType);

			CombineAssertions(() =>
			{
				AssertEquals("transaction1 is left as it was because it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction1.SRT_TransactionStatus);
				AssertEquals("transaction2 is changed to DEL because it was PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, transaction2.SRT_TransactionStatus);
				AssertEquals("transaction3 is changed to DEL because it was PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, transaction3.SRT_TransactionStatus);
				AssertEquals("transaction4 is changed to DEL because it was PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, transaction4.SRT_TransactionStatus);
				AssertEquals("transaction5 is left as it was because it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, transaction5.SRT_TransactionStatus);
				AssertEquals("transaction6 is left as it was because it was PND but it didn't have the correct internal reference number (type is correct)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction6.SRT_TransactionStatus);
				AssertEquals("transaction7 is left as it was because it was PND but it didn't have the correct internal reference type (number is correct)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction7.SRT_TransactionStatus);
			});
		}

		public void TestGetPendingRegLineTransactions()
		{
			var expectedInternalRefNum1 = "RefNumber1";
			var expectedInternalRefType1 = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
			var expectedInternalRefNum2 = "RefNumber2";
			var expectedInternalRefType2 = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;

			var transaction1 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction1.SRT_InternalReferenceNumber = expectedInternalRefNum1;
			transaction1.SRT_InternalReferenceType = expectedInternalRefType1;

			var transaction2 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction2.SRT_InternalReferenceNumber = expectedInternalRefNum2;
			transaction2.SRT_InternalReferenceType = expectedInternalRefType2;

			var transaction3 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
			transaction3.SRT_InternalReferenceNumber = expectedInternalRefNum2;
			transaction3.SRT_InternalReferenceType = expectedInternalRefType2;

			var transaction4 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction4.SRT_InternalReferenceNumber = expectedInternalRefNum2;
			transaction4.SRT_InternalReferenceType = expectedInternalRefType2;

			CombineAssertions(() =>
			{
				AssertEquals("transaction with Pending, reference and type correct", 2, GetPendingRegLineTransactions(Factory, expectedInternalRefNum2, expectedInternalRefType2).Length);
				AssertEquals("transaction with Pending, reference correct and type incorrect", 0, GetPendingRegLineTransactions(Factory, expectedInternalRefNum2, expectedInternalRefType1).Length);
				AssertEquals("transaction with Pending, reference incorrect and type correct", 0, GetPendingRegLineTransactions(Factory, expectedInternalRefNum1, expectedInternalRefType2).Length);
			});
		}

		public void TestGetConfirmedRegLineTransactions()
		{
			var expectedInternalRefNum1 = "InternalRefNumber1";
			var expectedInternalRefType1 = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
			var expectedRefNum1 = "RefNumber1";
			var expectedInternalRefNum2 = "InternalRefNumber2";
			var expectedInternalRefType2 = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			var expectedRefNum2 = "RefNumber2";

			var transaction1 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction1.SRT_InternalReferenceNumber = expectedInternalRefNum1;
			transaction1.SRT_InternalReferenceType = expectedInternalRefType1;

			var transaction2 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction2.SRT_InternalReferenceNumber = expectedInternalRefNum2;
			transaction2.SRT_InternalReferenceType = expectedInternalRefType2;

			var transaction3 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
			transaction3.SRT_InternalReferenceNumber = expectedInternalRefNum2;
			transaction3.SRT_InternalReferenceType = expectedInternalRefType2;

			var transaction4 = Factory.New<EUInterfaces.ICusTempStorageRegLineTransaction>();
			transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction4.SRT_InternalReferenceNumber = expectedInternalRefNum2;
			transaction4.SRT_InternalReferenceType = expectedInternalRefType2;
			transaction4.SRT_Reference = expectedRefNum1;

			CombineAssertions(() =>
			{
				AssertEquals("transaction with Confirmed, internal reference and type correct", 2, GetConfirmedRegLineTransactions(Factory, expectedInternalRefNum2, expectedInternalRefType2).Length);
				AssertEquals("transaction with Confirmed, internal reference correct and type incorrect", 0, GetConfirmedRegLineTransactions(Factory, expectedInternalRefNum2, expectedInternalRefType1).Length);
				AssertEquals("transaction with Confirmed, internal reference incorrect and type correct", 0, GetConfirmedRegLineTransactions(Factory, expectedInternalRefNum1, expectedInternalRefType2).Length);
				AssertEquals("transaction with Confirmed, internal reference, type and reference correct", 1, GetConfirmedRegLineTransactions(Factory, expectedInternalRefNum2, expectedInternalRefType2, expectedRefNum1).Length);
				AssertEquals("transaction with Confirmed, internal reference and type correct and reference incorrect", 0, GetConfirmedRegLineTransactions(Factory, expectedInternalRefNum2, expectedInternalRefType2, expectedRefNum2).Length);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithNoDocWithSpecificCode()
		{
			var neededPrevDocCode = "SUM";

			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = "BBB";
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "1234565";
			doc2.CSI_Code = "AAA";
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = "CCC";

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, "location", "ES0001");
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithNoRegHeaders()
		{
			var neededPrevDocCode = "SUM";

			var regHeader = SetUpTmpRegHeader(reference: "1234565");

			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "1234565";
			doc2.CSI_Code = "AAA";
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, "location", "ES0001");
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithNoRegHeaders_HavingFormatDocRef_DocRefShorterThan18()
		{
			var neededPrevDocCode = "SUM";

			var regHeader = SetUpTmpRegHeader(reference: "1234565AAA");

			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "1234565AAA";
			doc2.CSI_Code = "AAA";
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, "location", "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithNoRegHeaders_HavingFormatDocRef_DocRefLength18()
		{
			var neededPrevDocCode = "SUM";

			var regHeader = SetUpTmpRegHeader(reference: "123456578945612378AAA");

			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "123456578945612378";
			doc2.CSI_Code = "AAA";
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, "location", "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithNoRegHeaders_HavingFormatDocRef_DocRefLongerThan18()
		{
			var neededPrevDocCode = "SUM";

			var regHeader = SetUpTmpRegHeader(reference: "1234565789456123789AAA");

			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "1234565789456123789";
			doc2.CSI_Code = "AAA";
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, "location", "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithError()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref3");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001");
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "ES0001: Goods in TSD Number ref1 are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000002.\n\nPlease, set the correct location before submitting this declaration to Customs."
						: "ES0001: Goods in TSD Number ref2 are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithError_HavingJobNumber()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref3");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", jobNumber: "JOB001");
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "JOB001/ES0001: Goods in TSD Number ref1 are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000002.\n\nPlease, set the correct location before submitting this declaration to Customs."
						: "JOB001/ES0001: Goods in TSD Number ref2 are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1AAA");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2AAA");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref3AAA");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1AAA";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2AAA";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3AAA";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "ES0001: Goods in TSD Number ref1AAA are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000002.\n\nPlease, set the correct location before submitting this declaration to Customs."
						: "ES0001: Goods in TSD Number ref2AAA are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithError_HavingFormatDocRef_DocRefLength18()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref111111111111AAA");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref222222222222AAA");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref333333333333AAA");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref111111111111";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref222222222222AAA";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref333333333333";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "ES0001: Goods in TSD Number ref111111111111AAA are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000002.\n\nPlease, set the correct location before submitting this declaration to Customs."
						: "ES0001: Goods in TSD Number ref222222222222AAA are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithError_HavingFormatDocRef_DocRefLongerThan18()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref111111111111111AAA");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref222222222222222AAA");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref333333333333333AAA");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref111111111111111";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref222222222222222AAA";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref333333333333333";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "ES0001: Goods in TSD Number ref111111111111111AAA are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000002.\n\nPlease, set the correct location before submitting this declaration to Customs."
						: "ES0001: Goods in TSD Number ref222222222222222AAA are not stored in location 2801000001 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithOutRegHeaderAssociated_WithErrorForLAME()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", shouldHaveLAMEMessageError: true);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "ES0001: There is no record in the Temporary Storage Register for LAME Reception Certificate ref1. You might have mistaken the number.\n\nDo you want to cancel this declaration to check?"
						: "ES0001: There is no record in the Temporary Storage Register for LAME Reception Certificate ref2. You might have mistaken the number.\n\nDo you want to cancel this declaration to check?";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithErrorForLAME()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref3");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>()
			{
				new () { Document = doc1 },
				new () { Document = doc2 },
				new () { Document = doc3 }
			};

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", shouldHaveLAMEMessageError: true);
			CombineAssertions(() =>
			{
				AssertEquals("resultList is empty", 0, resultList.Count());
				AssertEquals("resultError is not empty", GetExpectedError(), resultError);
			});

			ZString GetExpectedError()
				=> resultError.Contains("ref1")
						? "ES0001: Goods in LAME Reception Certificate ref1 are not stored in location 2801000001. The correct location should be 9999000002.\n\nPlease, set the correct location before submitting this declaration to Customs."
						: "ES0001: Goods in LAME Reception Certificate ref2 are not stored in location 2801000001. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.";
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithError_ButNotCheck()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = "9999000002";
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = "9999000005";
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises3 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises3.SRP_Type = "ADT";
			premises3.SRP_CustomsLocation = expectedLocation;
			premises3.SRP_Code = "B";
			premises3.SRP_Description = "DESC3";
			premises3.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var regHeader3 = SetUpTmpRegHeader(reference: "ref3");
			regHeader3.SRH_SRP_Premises = premises3.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataDoc1 = new DeclarationDataToReserveTSGoods() { Document = doc1 };
			var declarationDataDoc2 = new DeclarationDataToReserveTSGoods() { Document = doc2 };
			var declarationDataDoc3 = new DeclarationDataToReserveTSGoods() { Document = doc3 };

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>() { declarationDataDoc1, declarationDataDoc2, declarationDataDoc3 };

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", checkPremises: false);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("resultList is not empty",
					new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>()
					{
						(declarationDataDoc1, regHeader1),
						(declarationDataDoc2, regHeader2),
						(declarationDataDoc3, regHeader3)
					},
					resultList);
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithoutError()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = expectedLocation;
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = expectedLocation;
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataDoc1 = new DeclarationDataToReserveTSGoods() { Document = doc1 };
			var declarationDataDoc2 = new DeclarationDataToReserveTSGoods() { Document = doc2 };
			var declarationDataDoc3 = new DeclarationDataToReserveTSGoods() { Document = doc3 };

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>() { declarationDataDoc1, declarationDataDoc2, declarationDataDoc3 };

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001");
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("resultList is not empty",
					new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>()
					{
						(declarationDataDoc1, regHeader1),
						(declarationDataDoc2, regHeader2)
					},
					resultList);
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithoutError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = expectedLocation;
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = expectedLocation;
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1AAA");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2AAA");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1AAA";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2AAA";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3AAA";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataDoc1 = new DeclarationDataToReserveTSGoods() { Document = doc1 };
			var declarationDataDoc2 = new DeclarationDataToReserveTSGoods() { Document = doc2 };
			var declarationDataDoc3 = new DeclarationDataToReserveTSGoods() { Document = doc3 };

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>() { declarationDataDoc1, declarationDataDoc2, declarationDataDoc3 };

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("resultList is not empty",
					new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>()
					{
						(declarationDataDoc1, regHeader1),
						(declarationDataDoc2, regHeader2)
					},
					resultList);
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithoutError_HavingFormatDocRef_DocRefLength18()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = expectedLocation;
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = expectedLocation;
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref111111111111111AAA");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref222222222222AAA");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref111111111111111";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref222222222222AAA";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref333333333333333";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataDoc1 = new DeclarationDataToReserveTSGoods() { Document = doc1 };
			var declarationDataDoc2 = new DeclarationDataToReserveTSGoods() { Document = doc2 };
			var declarationDataDoc3 = new DeclarationDataToReserveTSGoods() { Document = doc3 };

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>() { declarationDataDoc1, declarationDataDoc2, declarationDataDoc3 };

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("resultList is not empty",
					new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>()
					{
						(declarationDataDoc1, regHeader1),
						(declarationDataDoc2, regHeader2)
					},
					resultList);
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated_WithoutError_HavingFormatDocRef_DocRefLongerThan18()
		{
			var neededPrevDocCode = "SUM";
			var expectedLocation = "2801000001";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var premises1 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises1.SRP_Type = "ADT";
			premises1.SRP_CustomsLocation = expectedLocation;
			premises1.SRP_Code = "X";
			premises1.SRP_Description = "DESC";
			premises1.SRP_OA_PremisesAddress = orgAddress.PK;
			var premises2 = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises2.SRP_Type = "ADT";
			premises2.SRP_CustomsLocation = expectedLocation;
			premises2.SRP_Code = "A";
			premises2.SRP_Description = "DESC2";
			premises2.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader1 = SetUpTmpRegHeader(reference: "ref1111111111111111AAA");
			regHeader1.SRH_SRP_Premises = premises1.PK;

			var regHeader2 = SetUpTmpRegHeader(reference: "ref2222222222222222AAA");
			regHeader2.SRH_SRP_Premises = premises2.PK;

			var docsList = new List<CusSupportingInfo>();
			var doc1 = Factory.New<CusSupportingInfo>();
			doc1.CSI_ReferenceNumber = "ref1111111111111111";
			doc1.CSI_Code = neededPrevDocCode;
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_ReferenceNumber = "ref2222222222222222AAA";
			doc2.CSI_Code = neededPrevDocCode;
			var doc3 = Factory.New<CusSupportingInfo>();
			doc3.CSI_ReferenceNumber = "ref3333333333333333";
			doc3.CSI_Code = neededPrevDocCode;

			var declarationDataDoc1 = new DeclarationDataToReserveTSGoods() { Document = doc1 };
			var declarationDataDoc2 = new DeclarationDataToReserveTSGoods() { Document = doc2 };
			var declarationDataDoc3 = new DeclarationDataToReserveTSGoods() { Document = doc3 };

			var declarationDataToReserveTSGoodsList = new List<DeclarationDataToReserveTSGoods>() { declarationDataDoc1, declarationDataDoc2, declarationDataDoc3 };

			var (resultList, resultError) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(Factory, declarationDataToReserveTSGoodsList, neededPrevDocCode, expectedLocation, "ES0001", formatDocRef: FormatDocRefForTest);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("resultList is not empty",
					new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>()
					{
						(declarationDataDoc1, regHeader1),
						(declarationDataDoc2, regHeader2)
					},
					resultList);
				AssertEquals("resultError is empty", ZString.Empty, resultError);
			});
		}

		public void TestGetRegLineItemAssociatedToPreviousDocumentAndRegHeader()
		{
			var regHeader1 = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader1.SRH_AppCode = "AAA";
			regHeader1.SRH_Reference = RegHeaderReference;

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_CustomsStatus = "OPN";

			var regLineItem1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem1.SRI_GoodsItemNumber = 1;
			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_CustomsStatus = "CLS";

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;
			var regLineItemPivot2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot2.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

			var regLine3 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;
			regLine3.SRL_CustomsStatus = "OPN";

			var regLineItem3 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem3.SRI_GoodsItemNumber = 3;
			var regLineItemPivot3 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot3.SRV_SRI_Item = regLineItem3.PK;
			regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

			var regHeader2 = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader2.SRH_AppCode = "BBB";
			regHeader2.SRH_Reference = "reference2";

			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";

			var regLineItem4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem4.SRI_GoodsItemNumber = 1;
			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem4.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			var regLine5 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 5;
			regLine5.SRL_CustomsStatus = "OPN";

			var regLineItem5 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem5.SRI_GoodsItemNumber = 1;
			var regLineItemPivot5 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot5.SRV_SRI_Item = regLineItem5.PK;
			regLineItemPivot5.SRV_SRL_Line = regLine5.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("When asking for lineNo 1 in regheader1 we get regLineItem1", regLineItem1, GetRegLineItemAssociatedToPreviousDocumentAndRegHeader(Factory, 1, regHeader1));
				AssertNull("When asking for lineNo 2 in regheader1 we get null since that LineNo is in an item associated to a not OPN line", GetRegLineItemAssociatedToPreviousDocumentAndRegHeader(Factory, 2, regHeader1));
				AssertEquals("When asking for lineNo 3 in regheader1 we get regLineItem3", regLineItem3, GetRegLineItemAssociatedToPreviousDocumentAndRegHeader(Factory, 3, regHeader1));

				AssertEquals("When asking for lineNo 1 in regheader2 we get the first item found since there are 2 with that lineNo",
					true, new[] { regLineItem4, regLineItem5 }.Contains(GetRegLineItemAssociatedToPreviousDocumentAndRegHeader(Factory, 1, regHeader2)));
			});
		}

		public void TestGetRegLineFromLineItemWithPackTypeOrMarksOrVin()
		{
			var regHeader1 = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader1.SRH_AppCode = "AAA";
			regHeader1.SRH_Reference = RegHeaderReference;

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_CustomsStatus = "OPN";
			regLine1.SRL_PackageType = "NE";
			regLine1.SRL_PackageMarks = "AAAAAA";

			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_CustomsStatus = "OPN";
			regLine2.SRL_PackageType = "FR";
			regLine2.SRL_PackageMarks = "BBBBAA";

			var regLine3 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;
			regLine3.SRL_CustomsStatus = "OPN";
			regLine3.SRL_PackageType = "BX";
			regLine3.SRL_PackageMarks = "CCCCAA";

			var regLine4 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "DDDDAA";

			var regLine5 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 5;
			regLine5.SRL_CustomsStatus = "OPN";
			regLine5.SRL_PackageType = "BX";
			regLine5.SRL_PackageMarks = "CCBBAA";

			var regLine6 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine6.SRL_LineNumber = 6;
			regLine6.SRL_CustomsStatus = "CLS";
			regLine6.SRL_PackageType = "CT";
			regLine6.SRL_PackageMarks = "CCBBAA";

			var regLineItem1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem1.SRI_GoodsItemNumber = 1;

			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

			var regLineItemPivot2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot2.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

			var regLineItemPivot3 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot3.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			var regLineItemPivot5 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot5.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot5.SRV_SRL_Line = regLine5.PK;

			var regLineItemPivot6 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot6.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot6.SRV_SRL_Line = regLine6.PK;

			var regLine7 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine7.SRL_LineNumber = 7;
			regLine7.SRL_CustomsStatus = "OPN";
			regLine7.SRL_PackageType = "AA";
			regLine7.SRL_PackageMarks = "EEEEAA";

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;
			var regLineItemPivot7 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot7.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot7.SRV_SRL_Line = regLine7.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("When asking for regLines with type NE in regLineItem1 we get regLine1", regLine1, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "NE", ""));
				AssertEquals("When asking for regLines with type FR and vin BB in regLineItem1 we get regLine2", regLine2, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "FR", "BB"));
				AssertEquals("When asking for regLines with type BX and marks CCCCAA in regLineItem1 we get regLine3", regLine3, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "BX", "CCCCAA"));
				AssertEquals("When asking for regLines with type FR and vin DD in regLineItem1 we get regLine4", regLine4, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "FR", "DD"));
				AssertEquals("When asking for regLines with type BX and marks CCBBAA in regLineItem1 we get regLine5", regLine5, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "BX", "CCBBAA"));
				AssertNull("When asking for regLines with type CT we get null since there is no lines with that type associated to regLineItem1 but it's status is not OPN", GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "CT", ""));
				AssertEquals("When asking for regLines with type CT in regLineItem1 we get regLine6 when isAmendment is true", regLine6, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "CT", "", isAmendment: true));
				AssertNull("When asking for regLines with type BX and marks AAAA in regLineItem1 we get null because there are multiple lines with BX type but none with the specified marks", GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "BX", "AAAA"));
				AssertNull("When asking for regLines with type AA we get null since there are no lines with that type associated to regLineItem1", GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem1, "AA", ""));
				AssertEquals("When asking for regLines with type AA in regLineItem2 we get regLine7", regLine7, GetRegLineFromLineItemWithPackTypeOrMarksOrVin(Factory, regLineItem2, "AA", ""));
			});
		}

		public void TestReserveTemporaryStorageGoods()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "AAA";
			regHeader.SRH_Reference = RegHeaderReference;

			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "VQ";
			regLine1.SRL_CustomsStatus = "CLS";

			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 22, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, grossWeight: -2);

			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VG";
			regLine2.SRL_CustomsStatus = "CLS";

			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 5, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -4);
			var regLineTransaction6 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);

			var regLine3 = regHeader.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;
			regLine3.SRL_PackageType = "FR";

			var regLineTransaction7 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 1, grossWeight: 100, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction8 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, grossWeight: -2);

			var regLine4 = regHeader.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_PackageType = "BX";
			regLine4.SRL_CustomsStatus = "AAA";

			var regLineTransaction9 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 18, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction10 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, grossWeight: -2);

			var regLine5 = regHeader.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 5;
			regLine5.SRL_PackageType = "CT";

			var regLineTransaction11 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 23, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction12 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
			var regLineTransaction13 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -7, -7);

			var regLine6 = regHeader.CusTempStorageRegLines.AddNew();
			regLine6.SRL_LineNumber = 6;
			regLine6.SRL_PackageType = "CE";
			regLine6.SRL_CustomsStatus = "OPN";

			var regLineTransaction14 = SetUpTransaction(regLine6, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 230, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction15 = SetUpTransaction(regLine6, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -20, -20);

			var regLine7 = regHeader.CusTempStorageRegLines.AddNew();
			regLine7.SRL_LineNumber = 7;
			regLine7.SRL_PackageType = "BX";
			regLine7.SRL_CustomsStatus = "AAA";

			var regLineTransaction16 = SetUpTransaction(regLine7, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 18, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction17 = SetUpTransaction(regLine7, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, grossWeight: -2);

			var regLine8 = regHeader.CusTempStorageRegLines.AddNew();
			regLine8.SRL_LineNumber = 8;
			regLine8.SRL_PackageType = "CT";

			var regLineTransaction18 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 23, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction19 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
			var regLineTransaction20 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -7, -7);

			var dataToReserveList = new List<DataToReserveTSGoods>()
			{
				new () { RegLine = regLine1, TotalGrossWeight = 20.369m, TotalPackQty = 20, TotalEntryPackQty = 79, IsAdjustment = false },
				new () { RegLine = regLine2, TotalGrossWeight = 10.456m, TotalPackQty = 20, TotalEntryPackQty = 79, IsAdjustment = false },
				new () { RegLine = regLine3, TotalGrossWeight = 200m, TotalPackQty = 20, TotalEntryPackQty = 79, IsAdjustment = false },
				new () { RegLine = regLine4, TotalGrossWeight = 200m, TotalPackQty = 10, TotalEntryPackQty = 79, IsAdjustment = false },
				new () { RegLine = regLine5, TotalGrossWeight = 200m, TotalPackQty = 9, TotalEntryPackQty = 79, IsAdjustment = false },
				new () { RegLine = regLine6, TotalGrossWeight = 200m, TotalPackQty = 9, TotalEntryPackQty = 79, IsAdjustment = false },
				new () { RegLine = regLine7, TotalGrossWeight = 200m, TotalPackQty = 10, TotalEntryPackQty = 0, IsAdjustment = false },
				new () { RegLine = regLine8, TotalGrossWeight = 20m, TotalPackQty = 9, TotalEntryPackQty = 0, IsAdjustment = false },
			};

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: regLine1 has status CLS", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine2 has status CLS", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine3 has status empty", "", regLine3.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine4 has status AAA", "AAA", regLine4.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine5 has status empty", "", regLine5.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine6 has status OPN", "OPN", regLine6.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine7 has status AAA TotalEntryPackQty = 0", "AAA", regLine7.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine8 has status empty TotalEntryPackQty = 0", "", regLine8.SRL_CustomsStatus);

				ReserveTemporaryStorageGoods(InternalRefNum, InternalRefType, dataToReserveList);

				AssertNewTransactionToReserveGoods(regLine1, InternalRefNum, InternalRefType, 0, -20m);
				AssertNewTransactionToReserveGoods(regLine2, InternalRefNum, InternalRefType, 0, -10.456m);
				AssertNewTransactionToReserveGoods(regLine3, InternalRefNum, InternalRefType, -1, -100m);
				AssertNewTransactionToReserveGoods(regLine4, InternalRefNum, InternalRefType, -10, -16);
				AssertNewTransactionToReserveGoods(regLine5, InternalRefNum, InternalRefType, -9, -22m);
				AssertNewTransactionToReserveGoods(regLine6, InternalRefNum, InternalRefType, -9, -22.78481m);
				AssertNewTransactionToReserveGoods(regLine7, InternalRefNum, InternalRefType, -10, -16);
				AssertNewTransactionToReserveGoods(regLine8, InternalRefNum, InternalRefType, -9, -20m);
			});
		}

		public void TestReserveTemporaryStorageGoods_IsAmendment()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "AAA";
			regHeader.SRH_Reference = RegHeaderReference;

			var regLine6 = regHeader.CusTempStorageRegLines.AddNew();
			regLine6.SRL_LineNumber = 6;
			regLine6.SRL_PackageType = "FR";
			regLine6.SRL_CustomsStatus = "CLS";

			var regLineTransaction14 = SetUpTransaction(regLine6, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 50, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction15 = SetUpTransaction(regLine6, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, grossWeight: -2);

			var regLine7 = regHeader.CusTempStorageRegLines.AddNew();
			regLine7.SRL_LineNumber = 7;
			regLine7.SRL_PackageType = "VQ";
			regLine7.SRL_CustomsStatus = "CLS";

			var regLineTransaction16 = SetUpTransaction(regLine7, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 12, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction17 = SetUpTransaction(regLine7, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, grossWeight: -2);

			var regLine8 = regHeader.CusTempStorageRegLines.AddNew();
			regLine8.SRL_LineNumber = 8;
			regLine8.SRL_PackageType = "VG";

			var regLineTransaction18 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 5, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction19 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
			var regLineTransaction20 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -4);
			var regLineTransaction21 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);
			var regLineTransaction22 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 2);

			var regLine9 = regHeader.CusTempStorageRegLines.AddNew();
			regLine9.SRL_LineNumber = 9;
			regLine9.SRL_PackageType = "BX";
			regLine9.SRL_CustomsStatus = "AAA";

			var regLineTransaction23 = SetUpTransaction(regLine9, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 49, grossWeight: 18, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction24 = SetUpTransaction(regLine9, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -29, grossWeight: -2);
			var regLineTransaction25 = SetUpTransaction(regLine9, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -40, 0);

			var regLine10 = regHeader.CusTempStorageRegLines.AddNew();
			regLine10.SRL_LineNumber = 10;
			regLine10.SRL_PackageType = "CT";
			regLine10.SRL_CustomsStatus = "OPN";

			var regLineTransaction26 = SetUpTransaction(regLine10, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 23, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction27 = SetUpTransaction(regLine10, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 5, 6);
			var regLineTransaction28 = SetUpTransaction(regLine10, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, -7, -8);

			var regLine11 = regHeader.CusTempStorageRegLines.AddNew();
			regLine11.SRL_LineNumber = 11;
			regLine11.SRL_PackageType = "BX";
			regLine11.SRL_CustomsStatus = "AAA";

			var regLineTransaction29 = SetUpTransaction(regLine11, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 49, grossWeight: 18, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction30 = SetUpTransaction(regLine11, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -29, grossWeight: -2);
			var regLineTransaction31 = SetUpTransaction(regLine11, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -40, 0);

			var regLine12 = regHeader.CusTempStorageRegLines.AddNew();
			regLine12.SRL_LineNumber = 12;
			regLine12.SRL_PackageType = "BX";
			regLine12.SRL_CustomsStatus = "AAA";

			var regLineTransaction32 = SetUpTransaction(regLine12, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 49, grossWeight: 18, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction33 = SetUpTransaction(regLine12, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -29, grossWeight: -2);
			var regLineTransaction34 = SetUpTransaction(regLine12, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -40, 0);

			var dataToReserveList = new List<DataToReserveTSGoods>()
			{
				new () { RegLine = regLine6, TotalGrossWeight = 200m, TotalPackQty = 1, TotalEntryPackQty = 80, IsAdjustment = true },
				new () { RegLine = regLine7, TotalGrossWeight = 12.456m, TotalPackQty = 1, TotalEntryPackQty = 80, IsAdjustment = true },
				new () { RegLine = regLine8, TotalGrossWeight = 9.456m, TotalPackQty = 1, TotalEntryPackQty = 80, IsAdjustment = true },
				new () { RegLine = regLine9, TotalGrossWeight = 100m, TotalPackQty = 9, TotalEntryPackQty = 80, IsAdjustment = true },
				new () { RegLine = regLine10, TotalGrossWeight = 12m, TotalPackQty = 20, TotalEntryPackQty = 80, IsAdjustment = true },
				new () { RegLine = regLine11, TotalGrossWeight = 12m, TotalPackQty = 20, TotalEntryPackQty = 0, IsAdjustment = true },
				new () { RegLine = regLine12, TotalGrossWeight = 12m, TotalPackQty = 9, TotalEntryPackQty = 0, IsAdjustment = true },
			};

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: regLine6 has status CLS", "CLS", regLine6.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine7 has status CLS", "CLS", regLine7.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine8 has status empty", "", regLine8.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine9 has status AAA", "AAA", regLine9.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine10 has status OPN", "OPN", regLine10.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine11 has status AAA TotalEntryPackQty = 0", "AAA", regLine11.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine12 has status AAA TotalEntryPackQty = 0 and 0 packages remaning", "AAA", regLine12.SRL_CustomsStatus);

				ReserveTemporaryStorageGoods(InternalRefNum, InternalRefType, dataToReserveList);

				AssertNewTransactionToReserveGoods(regLine6, InternalRefNum, InternalRefType, -1, -50m, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine7, InternalRefNum, InternalRefType, 0, -10m, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine8, InternalRefNum, InternalRefType, 0, -1.456m, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine9, InternalRefNum, InternalRefType, 20, -16m, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine10, InternalRefNum, InternalRefType, -18, -1m, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine11, InternalRefNum, InternalRefType, 9, -10m, AdjustmentTransactionComment);
				AssertNewTransactionToReserveGoods(regLine12, InternalRefNum, InternalRefType, 20, -16m, AdjustmentTransactionComment);
			});
		}

		public void TestRestoreVehicles()
		{
			var regHeader1 = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader1.SRH_AppCode = "AAA";
			regHeader1.SRH_Reference = RegHeaderReference;

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			regLine1.SRL_PackageMarks = "AAAAAA";
			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "FR";
			regLine2.SRL_PackageMarks = "BBBBAA";
			regLine2.SRL_CustomsStatus = "CLS";
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 11, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLine3 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;
			regLine3.SRL_PackageType = "FR";
			regLine3.SRL_PackageMarks = "DDDDAA";
			var regLineTransaction5 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 12, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLine4 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "CCBBAA";
			var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 13, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction8 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLine5 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 5;
			regLine5.SRL_PackageType = "BX";
			regLine5.SRL_PackageMarks = "DDBBAA";
			var regLineTransaction9 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 14, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction10 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLineItem1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem1.SRI_GoodsItemNumber = 1;

			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

			var regLineItemPivot2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot2.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

			var regLineItemPivot3 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot3.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			var regLineItemPivot5 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot5.SRV_SRI_Item = regLineItem1.PK;
			regLineItemPivot5.SRV_SRL_Line = regLine5.PK;

			var regLine6 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine6.SRL_LineNumber = 6;
			regLine6.SRL_PackageType = "FR";
			regLine6.SRL_PackageMarks = "BBCCAA";
			regLine6.SRL_CustomsStatus = "AAA";
			var regLineTransaction11 = SetUpTransaction(regLine6, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 15, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction12 = SetUpTransaction(regLine6, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLine7 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine7.SRL_LineNumber = 7;
			regLine7.SRL_PackageType = "FR";
			regLine7.SRL_PackageMarks = "EEEEAA";
			var regLineTransaction13 = SetUpTransaction(regLine7, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 16, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction14 = SetUpTransaction(regLine7, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);

			var regLine8 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine8.SRL_LineNumber = 8;
			regLine8.SRL_PackageType = "FR";
			regLine8.SRL_PackageMarks = "EEEEBB";
			var regLineTransaction15 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 17, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
			var regLineTransaction16 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, grossWeight: 1);
			var regLineTransaction17 = SetUpTransaction(regLine8, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: 1);
			regLineTransaction17.SRT_InternalReferenceNumber = "AAAAAAA";

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot6 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot6.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot6.SRV_SRL_Line = regLine6.PK;

			var regLineItemPivot7 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot7.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot7.SRV_SRL_Line = regLine7.PK;

			var regLineItemPivot8 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot8.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot8.SRV_SRL_Line = regLine8.PK;

			Factory.Save();

			var vinListInDeclaration = new ZString[] { "DDDD", "EEEEAA" };

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: there are 2 transactions for regLine1", 2, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 2 transactions for regLine2", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 2 transactions for regLine3", 2, regLine3.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 2 transactions for regLine4", 2, regLine4.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 2 transactions for regLine5", 2, regLine5.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 2 transactions for regLine6", 2, regLine6.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 2 transactions for regLine7", 2, regLine7.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Prereq: there are 3 transactions for regLine8", 3, regLine8.CusTempStorageRegLineTransactions.Count);

				AssertEquals("Prereq: regLine2 has status CLS", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine4 has status empty", "", regLine4.SRL_CustomsStatus);
				AssertEquals("Prereq: regLine6 has status AAA", "AAA", regLine6.SRL_CustomsStatus);

				RestoreVehicles(Factory, InternalRefNum, InternalRefType, regLineItem1, vinListInDeclaration);

				AssertEquals("After calling restore for regLineItem1: there are 2 transactions for regLine1 since it's not FR", 2, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("After calling restore for regLineItem1: there are 3 transactions for regLine2 since it's FR and it's not in the vin list in declaration", 3, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertNewTransactionToRestoreVehicle(regLine2, InternalRefNum, InternalRefType, 11);
				AssertEquals("After calling restore for regLineItem1: there are 2 transactions for regLine3 since it's FR but it is in the vin list in declaration", 2, regLine3.CusTempStorageRegLineTransactions.Count);
				AssertEquals("After calling restore for regLineItem1: there are 2 transactions for regLine4 since it's FR and it's not in the vin list in declaration", 3, regLine4.CusTempStorageRegLineTransactions.Count);
				AssertNewTransactionToRestoreVehicle(regLine4, InternalRefNum, InternalRefType, 13);
				AssertEquals("After calling restore for regLineItem1: there are 2 transactions for regLine5 since it's not FR", 2, regLine5.CusTempStorageRegLineTransactions.Count);
				AssertEquals("After calling restore for regLineItem1: there are 2 transactions for regLine6 since it's not in regLineItem1", 2, regLine6.CusTempStorageRegLineTransactions.Count);
				AssertEquals("After calling restore for regLineItem1: there are 2 transactions for regLine7 since it's not in regLineItem1", 2, regLine7.CusTempStorageRegLineTransactions.Count);
				AssertEquals("After calling restore for regLineItem1: there are 3 transactions for regLine8 since it's not in regLineItem1", 3, regLine8.CusTempStorageRegLineTransactions.Count);

				RestoreVehicles(Factory, InternalRefNum, InternalRefType, regLineItem2, vinListInDeclaration);

				AssertEquals("After calling restore for regLineItem2: there are 2 transactions for regLine6 since it's FR and it's not in the vin list in declaration", 3, regLine6.CusTempStorageRegLineTransactions.Count);
				AssertNewTransactionToRestoreVehicle(regLine6, InternalRefNum, InternalRefType, 15);
				AssertEquals("After calling restore for regLineItem2: there are 2 transactions for regLine7 since it's FR but it is in the vin list in declaration", 2, regLine7.CusTempStorageRegLineTransactions.Count);
				AssertEquals("After calling restore for regLineItem2: there are 3 transactions for regLine8 since it's FR and it's not in the vin list in declaration but it has no CON transaction with the correct data", 3, regLine8.CusTempStorageRegLineTransactions.Count);
			});
		}

		#region ConfirmTemporaryStorageGoodsConsumptionIfNeeded
		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_EmptyGoodsLocation()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						ZString.Empty,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_LocationNotManagedInPremises()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						"9999000005",
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToCancel()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"AAA",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToConfirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpRefData();
				Factory.Save();

				var (_, orgAddress) = SetUpOrgHeader();

				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = "ADT";
				premises.SRP_CustomsLocation = LocationInEntry;
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;

				var regHeader1 = SetUpTmpRegHeader();

				var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine1.SRL_LineNumber = 1;
				regLine1.SRL_PackageType = "NE";
				var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine2.SRL_LineNumber = 2;
				regLine2.SRL_PackageType = "VQ";

				SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
				var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -6, bondAmount: -2.0m);
				var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2, bondAmount: -3.0m);

				SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
				var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5, bondAmount: -2.0m);
				var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -3.0m);
				var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, -10, -4, bondAmount: -4.0m);

				var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

				var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine3.SRL_LineNumber = 1;
				regLine3.SRL_PackageType = "AA";
				var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine4.SRL_LineNumber = 3;
				regLine4.SRL_PackageType = "VG";

				SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 10.0m);
				var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -2.0m);
				regLineTransaction6.SRT_Comments = OldComment;

				SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 6.0m);
				var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -4.0m);

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader1).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -15m);
					AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -2m);
					AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4, -5m);
					AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -6m, expectedOldComment: " - " + OldComment);
					AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, -3.6m);

					AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
					AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
					AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

					AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
					AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
					AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_WriteOff()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, orgAddress) = SetUpOrgHeader();

				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = "ADT";
				premises.SRP_CustomsLocation = LocationInEntry;
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;

				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
				var regLine = SetUpRegLine(regHeader);
				var regLine2 = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
				SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 5, grossWeight: 5, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 5.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -2);
				var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -1);
				var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -5, grossWeight: -5);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);
					AssertEquals("[PreReq] Bond Amount transaction3 is default", 0.0m, regLineTransaction3.SRT_BondAmount);

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
							LocationInEntry,
							Core.Constants.CountryCodes.Latvia,
							"BBB",
							InternalRefNum, InternalRefType,
							JobNumberForComment,
							PrevDocCode,
							GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
							MRNCode,
							CommentPrefix,
							JobNumberForComment,
							issueDate,
							releaseDate,
							WriteOffComment,
							((EnterpriseBusinessObject)regHeader).Logs,
							CustomsStatusToCancelTemporaryStoragePendingTransactions,
							CustomsStatusToNotCreateTemporaryStorageTransactions,
							CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);
					AssertEquals("Bond Amount transaction3 is calculated", -5.0m, regLineTransaction3.SRT_BondAmount);

					AssertEquals("3 Write off transactions in Guarantee are created", 3, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));

					var expectedComment = string.Format("Write-off TS {0} / DUA {1}", RegHeaderReference, MRNCode);
					var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", RegHeaderReference, 2.0m, releaseDate, expectedComment);
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", RegHeaderReference, 1.0m, releaseDate, expectedComment);
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(2), "Write-off transaction 3", RegHeaderReference, 5.0m, releaseDate, expectedComment);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_NoPendingAmount()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, orgAddress) = SetUpOrgHeader();

				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = "ADT";
				premises.SRP_CustomsLocation = LocationInEntry;
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;

				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGuaranteeForTempStorage(regHeader, 0.0m);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
							LocationInEntry,
							Core.Constants.CountryCodes.Latvia,
							"BBB",
							InternalRefNum, InternalRefType,
							JobNumberForComment,
							PrevDocCode,
							GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
							MRNCode,
							CommentPrefix,
							JobNumberForComment,
							issueDate,
							releaseDate,
							WriteOffComment,
							((EnterpriseBusinessObject)regHeader).Logs,
							CustomsStatusToCancelTemporaryStoragePendingTransactions,
							CustomsStatusToNotCreateTemporaryStorageTransactions,
							CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_PendingAmountPositive()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, orgAddress) = SetUpOrgHeader();

				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = "ADT";
				premises.SRP_CustomsLocation = LocationInEntry;
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;

				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGuaranteeForTempStorage(regHeader, 3.0m);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
							LocationInEntry,
							Core.Constants.CountryCodes.Latvia,
							"BBB",
							InternalRefNum, InternalRefType,
							JobNumberForComment,
							PrevDocCode,
							GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
							MRNCode,
							CommentPrefix,
							JobNumberForComment,
							issueDate,
							releaseDate,
							WriteOffComment,
							((EnterpriseBusinessObject)regHeader).Logs,
							CustomsStatusToCancelTemporaryStoragePendingTransactions,
							CustomsStatusToNotCreateTemporaryStorageTransactions,
							CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

					var expectedError = "|RES=Reference reference has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
					AssertEquals("New event in logs passed", expectedError, ((EnterpriseBusinessObject)regHeader).Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactions_StatusIsNotInList_DoNothing_WithPNDTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactions_StatusIsNotInList_DoNothing_WithCONTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, regHeaderReference: PrevDocReferenceShort);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, regHeaderReference: PrevDocReferenceLong);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, regHeaderReference: PrevDocReferenceLong + "AAA");

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_Confirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_Confirm_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, regHeaderReference: PrevDocReferenceShort);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_Confirm_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, regHeaderReference: PrevDocReferenceLong);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactions_StatusIsNotInList_Confirm_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, regHeaderReference: PrevDocReferenceLong + "AAA");

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactions_StatusIsInList_Empty()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						ZString.Empty,
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was not created (PDA)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactions_StatusIsInList_NotEmpty()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"CCC",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions);

					AssertEquals("regLineTransaction was not created (PDA)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		#endregion

		#region ConfirmTemporaryStorageGoodsConsumptionIfNeeded IsLAME

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(isLAME: true);

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldntDoAction_EmptyGoodsLocation()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(isLAME: true);

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						ZString.Empty,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldntDoAction_LocationNotManagedInPremises()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(isLAME: true);

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						"9999000005",
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldDoActionWithCustomsStatusToCancel()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, _, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(isLAME: true);

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"AAA",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldDoActionWithCustomsStatusToConfirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpRefData();
				Factory.Save();

				var (_, orgAddress) = SetUpOrgHeader();

				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = "LAM";
				premises.SRP_CustomsLocation = LocationInEntry;
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;

				var regHeader1 = SetUpTmpRegHeader();

				var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine1.SRL_LineNumber = 1;
				regLine1.SRL_PackageType = "NE";
				var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine2.SRL_LineNumber = 2;
				regLine2.SRL_PackageType = "VQ";

				SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
				var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -6, bondAmount: -2.0m);
				var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2, bondAmount: -3.0m);

				SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
				var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5, bondAmount: -2.0m);
				var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -3.0m);
				var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, -10, -4, bondAmount: -4.0m);

				var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

				var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine3.SRL_LineNumber = 1;
				regLine3.SRL_PackageType = "AA";
				var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine4.SRL_LineNumber = 3;
				regLine4.SRL_PackageType = "VG";

				SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 10.0m);
				var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -2.0m);
				regLineTransaction6.SRT_Comments = OldComment;

				SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 6.0m);
				var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -4.0m);

				CombineAssertions(() =>
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader1).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -2.0m);
					AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -3.0m);
					AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4, -3.0m);
					AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -2.0m, expectedOldComment: " - " + OldComment);
					AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, -4.0m);

					AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
					AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
					AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

					AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
					AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
					AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_NoWriteOff()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, orgAddress) = SetUpOrgHeader();

				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = "LAM";
				premises.SRP_CustomsLocation = LocationInEntry;
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;

				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
				var regLine = SetUpRegLine(regHeader);
				var regLine2 = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
				SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 5, grossWeight: 5, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 5.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -2);
				var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -1);
				var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -5, grossWeight: -5);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);
					AssertEquals("[PreReq] Bond Amount transaction3 is default", 0.0m, regLineTransaction3.SRT_BondAmount);

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
							LocationInEntry,
							Core.Constants.CountryCodes.Latvia,
							"BBB",
							InternalRefNum, InternalRefType,
							JobNumberForComment,
							PrevDocCode,
							GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
							MRNCode,
							CommentPrefix,
							JobNumberForComment,
							issueDate,
							releaseDate,
							WriteOffComment,
							((EnterpriseBusinessObject)regHeader).Logs,
							CustomsStatusToCancelTemporaryStoragePendingTransactions,
							CustomsStatusToNotCreateTemporaryStorageTransactions,
							CustomsStatusToConfirmTemporaryStoragePendingTransactions,
							isLAME: true);

					AssertEquals("Bond Amount transaction1 is not calculated", -0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("Bond Amount transaction2 is not calculated", 0m, regLineTransaction2.SRT_BondAmount);
					AssertEquals("Bond Amount transaction3 is not calculated", 0m, regLineTransaction3.SRT_BondAmount);

					AssertEquals("No Write off transactions in Guarantee are created", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldNotAddPNDTransactions_StatusIsNotInList_DoNothing_WithPNDTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(isLAME: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldNotAddPNDTransactions_StatusIsNotInList_DoNothing_WithCONTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(isLAME: true);
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true, regHeaderReference: PrevDocReferenceShort);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true, regHeaderReference: PrevDocReferenceLong);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_DoNotConfirm_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true, regHeaderReference: PrevDocReferenceLong + "AAA");

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"DDD",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_Confirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_Confirm_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true, regHeaderReference: PrevDocReferenceShort);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_Confirm_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true, regHeaderReference: PrevDocReferenceLong);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldAddPNDTransactions_StatusIsNotInList_Confirm_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true, regHeaderReference: PrevDocReferenceLong + "AAA");

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"BBB",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true,
						formatDocRef: FormatDocRefForTest);

					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldNotAddPNDTransactions_StatusIsInList_Empty()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						ZString.Empty,
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was not created (PDA)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_IsLAME_ShouldNotAddPNDTransactions_StatusIsInList_NotEmpty()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, isLAME: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						LocationInEntry,
						Core.Constants.CountryCodes.Latvia,
						"CCC",
						InternalRefNum, InternalRefType,
						JobNumberForComment,
						PrevDocCode,
						GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18,
						MRNCode,
						CommentPrefix,
						JobNumberForComment,
						issueDate,
						releaseDate,
						WriteOffComment,
						((EnterpriseBusinessObject)regHeader).Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: true);

					AssertEquals("regLineTransaction was not created (PDA)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		#endregion

		#region ConfirmTemporaryStorageGoodsConsumption

		public void TestConfirmTemporaryStorageGoodsConsumption_NonBulkPackages()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_CustomsStatus = "CLS";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "NE";

			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
			var regLineTransaction1 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, bondAmount: -2.0m);
			var regLineTransaction2 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -4, bondAmount: -3.0m);
			regLineTransaction2.SRT_Comments = OldComment;
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, packageQty: -5, bondAmount: -4.0m);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -6, bondAmount: -5.0m);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_CustomsStatus = "CLS";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "AA";

			SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction5 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, packageQty: -6, grossWeight: -6, bondAmount: -2.0m);
			var regLineTransaction6 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -6, grossWeight: -6, bondAmount: -3.0m);

			var regLine5 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 3;
			regLine5.SRL_PackageType = "AA";
			SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction7 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -6, grossWeight: 6, bondAmount: -3.0m);

			CombineAssertions(() =>
			{
				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader1).Logs);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -10m);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -2m, expectedOldComment: " - " + OldComment);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertEquals("regLineTransaction4 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction4.SRT_TransactionStatus);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -7.2m);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, 0m);

				AssertEquals("regLine2 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine4 CustomsStatus is changed to OPN because the PackagsRemaining is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regLine5 CustomsStatus is changed to OPN because the PackagsRemaining is not 0", "OPN", regLine5.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_BulkPackages()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_CustomsStatus = "CLS";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
			var regLineTransaction1 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, grossWeight: -10, bondAmount: -2.0m);
			regLineTransaction1.SRT_Comments = OldComment;
			var regLineTransaction2 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, grossWeight: -4, bondAmount: -3.0m);
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, grossWeight: -5, bondAmount: -4.0m);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: -6, bondAmount: -5.0m);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");
			regHeader2.SRH_Status = "CLS";

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_CustomsStatus = "CLS";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";
			regLine4.SRL_CustomsStatus = "CLS";

			SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction5 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, grossWeight: -6, bondAmount: -3.0m);
			var regLineTransaction6 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, grossWeight: -6, bondAmount: -2.0m);

			var regLine5 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 3;
			regLine5.SRL_PackageType = "VG";
			SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction7 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -6, grossWeight: 6, bondAmount: -3.0m);

			CombineAssertions(() =>
			{
				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader1).Logs);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -10m, expectedOldComment: " - " + OldComment);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -2m);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertEquals("regLineTransaction4 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction4.SRT_TransactionStatus);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -7.2m);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, 0m);

				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regLine5 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine5.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_AllPackages()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -6, bondAmount: -2.0m);
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2, bondAmount: -3.0m);

			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5, bondAmount: -2.0m);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -3.0m);
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, -10, -4, bondAmount: -4.0m);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_PackageType = "AA";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";

			SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 10.0m);
			var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -2.0m);
			regLineTransaction6.SRT_Comments = OldComment;

			SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 6.0m);
			var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -4.0m);

			CombineAssertions(() =>
			{
				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader1).Logs);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -15m);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -2m);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4, -5m);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -6m, expectedOldComment: " - " + OldComment);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, -3.6m);

				AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_WriteOff()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
			var regLine = SetUpRegLine(regHeader);
			var regLine2 = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 5, grossWeight: 5, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 5.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -1);
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -5, grossWeight: -5);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction3 is default", 0.0m, regLineTransaction3.SRT_BondAmount);

				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader).Logs);

				AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);
				AssertEquals("Bond Amount transaction3 is calculated", -5.0m, regLineTransaction3.SRT_BondAmount);

				AssertEquals("3 Write off transactions in Guarantee are created", 3, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));

				var expectedComment = string.Format("Write-off TS {0} / DUA {1}", RegHeaderReference, MRNCode);
				var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", RegHeaderReference, 2.0m, releaseDate, expectedComment);
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", RegHeaderReference, 1.0m, releaseDate, expectedComment);
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(2), "Write-off transaction 3", RegHeaderReference, 5.0m, releaseDate, expectedComment);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_NoPendingAmount()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, 0.0m);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader).Logs);

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_PendingAmountPositive()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, 3.0m);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader).Logs);

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

				var expectedError = "|RES=Reference reference has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
				AssertEquals("New event in logs passed", expectedError, ((EnterpriseBusinessObject)regHeader).Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			});
		}

		#endregion

		#region ConfirmTemporaryStorageGoodsConsumption IsLAME

		public void TestConfirmTemporaryStorageGoodsConsumption_IsLAME_NonBulkPackages()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_CustomsStatus = "CLS";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "NE";

			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
			var regLineTransaction1 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -10, bondAmount: -2.0m);
			var regLineTransaction2 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -4, bondAmount: -3.0m);
			regLineTransaction2.SRT_Comments = OldComment;
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, packageQty: -5, bondAmount: -4.0m);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -6, bondAmount: -5.0m);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_CustomsStatus = "CLS";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "AA";

			SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction5 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, packageQty: -6, grossWeight: -6, bondAmount: -2.0m);
			var regLineTransaction6 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -6, grossWeight: -6, bondAmount: -3.0m);

			var regLine5 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 3;
			regLine5.SRL_PackageType = "AA";
			SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction7 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -6, grossWeight: 6, bondAmount: -3.0m);

			CombineAssertions(() =>
			{
				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader1).Logs, isLAME: true);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -2.0m);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -3.0m, expectedOldComment: " - " + OldComment);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertEquals("regLineTransaction4 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction4.SRT_TransactionStatus);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -3.0m);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, -3.0m);

				AssertEquals("regLine2 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine4 CustomsStatus is changed to OPN because the PackagsRemaining is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regLine5 CustomsStatus is changed to OPN because the PackagsRemaining is not 0", "OPN", regLine5.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_IsLAME_BulkPackages()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_CustomsStatus = "CLS";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
			var regLineTransaction1 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, grossWeight: -10, bondAmount: -2.0m);
			regLineTransaction1.SRT_Comments = OldComment;
			var regLineTransaction2 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, grossWeight: -4, bondAmount: -3.0m);
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, grossWeight: -5, bondAmount: -4.0m);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, grossWeight: -6, bondAmount: -5.0m);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");
			regHeader2.SRH_Status = "CLS";

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_CustomsStatus = "CLS";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";
			regLine4.SRL_CustomsStatus = "CLS";

			SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction5 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, grossWeight: -6, bondAmount: -3.0m);
			var regLineTransaction6 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, grossWeight: -6, bondAmount: -2.0m);

			var regLine5 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 3;
			regLine5.SRL_PackageType = "VG";
			SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction7 = SetUpTransaction(regLine5, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -6, grossWeight: 6, bondAmount: -3.0m);

			CombineAssertions(() =>
			{
				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader1).Logs, isLAME: true);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -2.0m, expectedOldComment: " - " + OldComment);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -3.0m);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertEquals("regLineTransaction4 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction4.SRT_TransactionStatus);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -2.0m);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, -3.0m);

				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regLine5 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine5.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_IsLAME_AllPackages()
		{
			SetUpRefData();
			Factory.Save();

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 20.0m);
			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -6, bondAmount: -2.0m);
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2, bondAmount: -3.0m);

			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 20, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 12.0m);
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5, bondAmount: -2.0m);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -3.0m);
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, -10, -4, bondAmount: -4.0m);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_PackageType = "AA";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";

			SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 10.0m);
			var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -2.0m);
			regLineTransaction6.SRT_Comments = OldComment;

			SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 10, grossWeight: 10, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 6.0m);
			var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6, bondAmount: -4.0m);

			CombineAssertions(() =>
			{
				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader1).Logs, isLAME: true);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, -2.0m);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2, -3.0m);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4, -3.0m);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6, -2.0m, expectedOldComment: " - " + OldComment);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7, -4.0m);

				AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}

		public void TestConfirmTemporaryStorageGoodsConsumption_IsLAME_NoWriteOff()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
			var regLine = SetUpRegLine(regHeader);
			var regLine2 = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
			SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 5, grossWeight: 5, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 5.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -1);
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -5, grossWeight: -5);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction3 is default", 0.0m, regLineTransaction3.SRT_BondAmount);

				ConfirmTemporaryStorageGoodsConsumption(Factory, InternalRefNum, InternalRefType, MRNCode, CommentPrefix, JobNumberForComment, issueDate, releaseDate, WriteOffComment, ((EnterpriseBusinessObject)regHeader).Logs, isLAME: true);

				AssertEquals("Bond Amount transaction1 is not calculated", 0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is not calculated", 0m, regLineTransaction2.SRT_BondAmount);
				AssertEquals("Bond Amount transaction3 is not calculated", 0m, regLineTransaction3.SRT_BondAmount);

				AssertEquals("0 Write off transactions in Guarantee are created", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			});
		}

		#endregion

		#region ConfirmNewADJTransactionBeforeSaving

		public void TestConfirmNewADJTransactionBeforeSaving_WithoutWriteOff_NoPendingAmount()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, 0.0m);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);

				AssertEquals("No Message Error", ZString.Empty, result);
				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);
			});
		}

		public void TestConfirmNewADJTransactionBeforeSaving_WithoutWriteOff_BondAmountZero()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -3, grossWeight: -3, bondAmount: -3m);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 0, grossWeight: 0);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction2);

				AssertEquals("No Message Error", ZString.Empty, result);
				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction2 is calculated", 0m, regLineTransaction2.SRT_BondAmount);
				AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);
			});
		}

		public void TestConfirmNewADJTransactionBeforeSaving_WithoutWriteOff_BondAmountZeroWhenGrossWeightIsPositive()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -3, grossWeight: -3, bondAmount: -3m);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 0, grossWeight: 5);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction2);

				AssertEquals("No Message Error", ZString.Empty, result);
				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction2 is set to 0", 0m, regLineTransaction2.SRT_BondAmount);
				AssertEquals("Line's status is changed to OPN when remaining gross weight is not 0", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to OPN when not all regLines in regHeader are CLS", "OPN", regHeader.SRH_Status);
			});
		}

		public void TestConfirmNewADJTransactionBeforeSaving_WithWriteOff_PendingAmountLessThanBondAmount()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -2.0m);
			var regLine = SetUpRegLine(regHeader);
			var regLine2 = SetUpRegLine(regHeader);
			regLine2.SRL_CustomsStatus = "CLS";

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
			regLineTransaction1.SRT_Reference = MRNCode;
			regLineTransaction1.SRT_ReferenceType = "MRN";
			regLineTransaction1.SRT_PhysicalInOutDate = releaseDate.ToOffset();

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);

				AssertEquals("No Message Error", ZString.Empty, result);

				var expectedComment = string.Format("Write-off TS {0} / MRN {1}", RegHeaderReference, MRNCode);
				var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
				AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 2.0m, releaseDate, expectedComment);

				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);
			});
		}

		public void TestConfirmNewADJTransactionBeforeSaving_WithWriteOff_PendingAmountSameAsBondAmount()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -0.45m);
			var regLine = SetUpRegLine(regHeader);
			regLine.SRL_CustomsStatus = "CLS";
			regHeader.SRH_Status = "CLS";

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
			regLineTransaction1.SRT_Reference = MRNCode;
			regLineTransaction1.SRT_ReferenceType = "MRN";
			regLineTransaction1.SRT_PhysicalInOutDate = releaseDate.ToOffset();

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);

				AssertEquals("No Message Error", ZString.Empty, result);

				var expectedComment = string.Format("Write-off TS {0} / MRN {1}", RegHeaderReference, MRNCode);
				var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
				AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 0.45m, releaseDate, expectedComment);

				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction1 is calculated", -0.45m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Line's status is changed to OPN when remaining gross weight is not 0", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to OPN when not all regLines in regHeader are CLS", "OPN", regHeader.SRH_Status);
			});
		}

		public void TestConfirmNewADJTransactionBeforeSaving_WithWriteOff_PendingAmountMoreThanAsBondAmount()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
			var regLine = SetUpRegLine(regHeader);
			regLine.SRL_CustomsStatus = "OPN";
			var regLine2 = SetUpRegLine(regHeader);
			regLine2.SRL_CustomsStatus = "OPN";
			regHeader.SRH_Status = "CLS";

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
			regLineTransaction1.SRT_Reference = MRNCode;
			regLineTransaction1.SRT_ReferenceType = "NUM";
			regLineTransaction1.SRT_PhysicalInOutDate = releaseDate.ToOffset();

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);

				AssertEquals("No Message Error", ZString.Empty, result);

				var expectedComment = string.Format("Write-off TS {0} / NUM {1}", RegHeaderReference, MRNCode);
				var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
				AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 3.0m, releaseDate, expectedComment);

				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to OPN when not all regLines in regHeader are CLS", "OPN", regHeader.SRH_Status);
			});
		}

		public void TestConfirmNewADJTransactionBeforeSaving_WithWriteOffError()
		{
			var expectedError = "Reference " + RegHeaderReference + " has a positive balance of 3.46 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";

			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, 3.45698m);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);

				AssertEquals("There is a Message Error", expectedError, result);

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);
			});
		}

		public void TestResetNewADJTransactionWhenSavingError()
		{
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
			var regLine = SetUpRegLine(regHeader);
			regLine.SRL_CustomsStatus = "OPN";

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
			regLineTransaction1.SRT_Reference = MRNCode;
			regLineTransaction1.SRT_ReferenceType = "NUM";
			regLineTransaction1.SRT_PhysicalInOutDate = releaseDate.ToOffset();

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				var result = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);

				AssertEquals("No Message Error", ZString.Empty, result);

				var expectedComment = string.Format("Write-off TS {0} / NUM {1}", RegHeaderReference, MRNCode);
				var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
				AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 3.0m, releaseDate, expectedComment);

				AssertEquals("SRT_TransactionStatus was changed to CON", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

				ResetNewADJTransactionWhenSavingError(regLineTransaction1);

				AssertEquals("Bond Amount transaction1 is reset to 0 when calling reset and transaction was not saved to database", 0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Write off transactions in Guarantee are removed when calling reset and they were not saved to database", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));

				_ = ConfirmNewADJTransactionBeforeSaving(regLineTransaction1);
				Factory.Save();
				AssertEquals("Bond Amount transaction1 is calculated and we save it", -3.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("New Write off transaction is created in Guarantee and we save it", 1, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));

				ResetNewADJTransactionWhenSavingError(regLineTransaction1);
				AssertEquals("Bond Amount transaction1 is not changed when calling reset and transaction was saved to database", -3.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Write off transactions in Guarantee are left as is when calling reset and transaction was saved to database", 1, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			});
		}

		#endregion

		#region ManageTemporaryStorageCancelationWhenProcessResponse

		public void TestManageTemporaryStorageCancelationWhenProcessResponseTSNotEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (_, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numGuaranteeTransactions);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseLocationNotManagedInPremises()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", "NewLocation", "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numGuaranteeTransactions);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseShouldNotManageTemporaryStorageCancelation()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				regLineTransaction.SRT_PackageQty = 3;
				regLineTransaction.SRT_GrossWeight = 3.0m;
				regLineTransaction.SRT_BondAmount = -2.0m;
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldNotManageTemporaryStorageCancelation: true);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse when shouldNotManageTemporaryStorageCancelation is true not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse when shouldNotManageTemporaryStorageCancelation is true not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse when shouldNotManageTemporaryStorageCancelation is true not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse when shouldNotManageTemporaryStorageCancelation is true not change", 1, numGuaranteeTransactions);

					ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldNotManageTemporaryStorageCancelation: false);
					AssertEquals("Number of transactions first RegLine after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 2, regLine.CusTempStorageRegLineTransactions.Count);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 2, guarantee.CusGuaranteeLineTransactions.Count);
					AssertEquals("Transaction after ManageTemporaryStorageCancelationWhenProcessResponse status does not change", "CON", regLineTransaction.SRT_TransactionStatus);
					AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine, bondAmound: 2.0m, packageQty: 3, grossWeight: 3.00m);
					AssertGuaranteeForManageTemporaryStorageCancelationWhenProcessResponse(guarantee, tranValue: -2.0m);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseNoTransactions()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse(createTransaction: false);
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", "25ES00999912345678", "TPref:", "GPref", ZDateTime.BrettsBirthday);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 0, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numGuaranteeTransactions);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseInternalReferenceNotMatch()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "NewReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numGuaranteeTransactions);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseInternalTypeNotMatch()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DAE", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numGuaranteeTransactions);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseMRNNotMatch()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (_, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", "25ES00999912345678", "TPref:", "GPref", ZDateTime.BrettsBirthday);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse not change", 1, numGuaranteeTransactions);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseShouldCancelPendingTransactions()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLine1, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse(transactionType: CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				var regLineTransaction2 = SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending);

				var regLine2 = SetUpRegLineForManageTemporaryStorageCancelationWhenProcessResponse(regHeader, 2);
				var regLineTransaction3 = SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending);

				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldCancelPendingTransactions: false);
				CombineAssertions(() =>
				{
					AssertEquals("Transaction1 Status after ManageTemporaryStorageCancelationWhenProcessResponse with shouldCancelPendingTransactions false", "PND", regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("Transaction2 Status after ManageTemporaryStorageCancelationWhenProcessResponse with shouldCancelPendingTransactions false", "PND", regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("Transaction3 Status after ManageTemporaryStorageCancelationWhenProcessResponse with shouldCancelPendingTransactions false", "PND", regLineTransaction3.SRT_TransactionStatus);

					ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldCancelPendingTransactions: true);
					AssertEquals("Transaction1 Status after ManageTemporaryStorageCancelationWhenProcessResponse with shouldCancelPendingTransactions true", "DEL", regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("Transaction2 Status after ManageTemporaryStorageCancelationWhenProcessResponse with shouldCancelPendingTransactions true", "DEL", regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("Transaction3 Status after ManageTemporaryStorageCancelationWhenProcessResponse with shouldCancelPendingTransactions true", "DEL", regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseShouldCheckTransactionsAlreadyCanceled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction, regLine, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse(true);
				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldCheckTransactionsAlreadyCanceled: true);

				var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
				var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is zero not change", 1, numTransactions);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is zero not change", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is zero not change", "CLS", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is zero not change", 1, numGuaranteeTransactions);

					regLineTransaction.SRT_PackageQty = 3;
					regLineTransaction.SRT_GrossWeight = -3.0m;
					regLineTransaction.SRT_BondAmount = -2.0m;
					ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldCheckTransactionsAlreadyCanceled: true);
					AssertEquals("Number of transactions first RegLine after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is less than zero and no Canceled transaction exists has a new transaction", 2, regLine.CusTempStorageRegLineTransactions.Count);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is less than zero and no Canceled transaction exists is open", "OPN", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is less than zero and no Canceled transaction exists is open", "OPN", regLine.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is less than zero and no Canceled transaction exists has a new transaction", 2, guarantee.CusGuaranteeLineTransactions.Count);
					AssertEquals("Transaction after ManageTemporaryStorageCancelationWhenProcessResponse if total gross weight is less than zero and no Canceled transaction exists status does not change", "CON", regLineTransaction.SRT_TransactionStatus);
					AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine, bondAmound: 2.0m, packageQty: 3, grossWeight: 3.00m);
					AssertGuaranteeForManageTemporaryStorageCancelationWhenProcessResponse(guarantee, tranValue: -2.0m);

					regLineTransaction.SRT_GrossWeight = -5.0m;
					regLineTransaction.SRT_BondAmount = -5.0m;
					ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldCheckTransactionsAlreadyCanceled: true);
					AssertEquals("Number of transactions first RegLine if Canceled transaction exists not change", 2, regLine.CusTempStorageRegLineTransactions.Count);
					AssertEquals("Number of guarantee transactions if Canceled transaction exists not change", 2, guarantee.CusGuaranteeLineTransactions.Count);

					ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, shouldCheckTransactionsAlreadyCanceled: false);
					AssertEquals("Number of transactions first RegLine if ShouldCheckTransactionsAlreadyCanceled change", 3, regLine.CusTempStorageRegLineTransactions.Count);
					AssertEquals("Number of guarantee transactions after ShouldCheckTransactionsAlreadyCanceled change", 3, guarantee.CusGuaranteeLineTransactions.Count);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseCONTransactions()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLine1, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse();
				regLineTransaction1.SRT_PackageQty = 3;
				regLineTransaction1.SRT_GrossWeight = -3.0m;
				regLineTransaction1.SRT_BondAmount = -2.0m;

				var regLineTransaction2 = SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine1);
				regLineTransaction2.SRT_PackageQty = 3;
				regLineTransaction2.SRT_GrossWeight = -3.0m;
				regLineTransaction2.SRT_BondAmount = -2.0m;

				var regLine2 = SetUpRegLineForManageTemporaryStorageCancelationWhenProcessResponse(regHeader, 2);
				var regLineTransaction3 = SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine2);
				regLineTransaction3.SRT_PackageQty = 3;
				regLineTransaction3.SRT_GrossWeight = -3.0m;
				regLineTransaction3.SRT_BondAmount = ZDecimal.Zero;

				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;

				CombineAssertions("[PreReq]", () =>
				{
					AssertEquals("Number of transactions first RegLine before ManageTemporaryStorageCancelationWhenProcessResponse", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("Number of transactions second RegLine before ManageTemporaryStorageCancelationWhenProcessResponse", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("SRH_Status before ManageTemporaryStorageCancelationWhenProcessResponse is CLS", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus before ManageTemporaryStorageCancelationWhenProcessResponse is CLS", "CLS", regLine1.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions ManageTemporaryStorageCancelationWhenProcessResponse", 1, guarantee.CusGuaranteeLineTransactions.Count);
				});

				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday);
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions first RegLine after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("Number of transactions second RegLine after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regHeader.SRH_Status);
					AssertEquals("RegLine1 SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regLine1.SRL_CustomsStatus);
					AssertEquals("RegLine2 SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regLine2.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 2, guarantee.CusGuaranteeLineTransactions.Count);
					AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine1, bondAmound: 4.0m, packageQty: 6, grossWeight: 6.00m);
					AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine2, bondAmound: 0.0m, packageQty: 3, grossWeight: 3.00m);
					AssertGuaranteeForManageTemporaryStorageCancelationWhenProcessResponse(guarantee, tranValue: -4.0m);
				});
			}
		}

		public void TestManageTemporaryStorageCancelationWhenProcessResponseCONTransactions_PremiseTypeLAM()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLine1, regHeader) = SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse(premiseType: CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility);
				regLineTransaction1.SRT_PackageQty = 3;
				regLineTransaction1.SRT_GrossWeight = -3.0m;
				regLineTransaction1.SRT_BondAmount = -2.0m;

				var regLineTransaction2 = SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine1);
				regLineTransaction2.SRT_PackageQty = 3;
				regLineTransaction2.SRT_GrossWeight = -3.0m;
				regLineTransaction2.SRT_BondAmount = -2.0m;

				var regLine2 = SetUpRegLineForManageTemporaryStorageCancelationWhenProcessResponse(regHeader, 2);
				var regLineTransaction3 = SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine2);
				regLineTransaction3.SRT_PackageQty = 3;
				regLineTransaction3.SRT_GrossWeight = -3.0m;
				regLineTransaction3.SRT_BondAmount = ZDecimal.Zero;

				var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;

				CombineAssertions("[PreReq]", () =>
				{
					AssertEquals("Number of transactions first RegLine before ManageTemporaryStorageCancelationWhenProcessResponse", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("Number of transactions second RegLine before ManageTemporaryStorageCancelationWhenProcessResponse", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("SRH_Status before ManageTemporaryStorageCancelationWhenProcessResponse is CLS", "CLS", regHeader.SRH_Status);
					AssertEquals("SRL_CustomsStatus before ManageTemporaryStorageCancelationWhenProcessResponse is CLS", "CLS", regLine1.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions ManageTemporaryStorageCancelationWhenProcessResponse", 1, guarantee.CusGuaranteeLineTransactions.Count);
				});

				ManageTemporaryStorageCancelationWhenProcessResponse(Factory, "EU", LocationInEntry, "TestReference", "CommentReference", "DUA", MRNCode, "TPref:", "GPref", ZDateTime.BrettsBirthday, CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility);
				CombineAssertions(() =>
				{
					AssertEquals("Number of transactions first RegLine after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("Number of transactions second RegLine after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("SRH_Status after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regHeader.SRH_Status);
					AssertEquals("RegLine1 SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regLine1.SRL_CustomsStatus);
					AssertEquals("RegLine2 SRL_CustomsStatus after ManageTemporaryStorageCancelationWhenProcessResponse is open", "OPN", regLine2.SRL_CustomsStatus);
					AssertEquals("Number of guarantee transactions after ManageTemporaryStorageCancelationWhenProcessResponse has a new transaction", 2, guarantee.CusGuaranteeLineTransactions.Count);
					AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine1, bondAmound: 4.0m, packageQty: 6, grossWeight: 6.00m);
					AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine2, bondAmound: 0.0m, packageQty: 3, grossWeight: 3.00m);
					AssertGuaranteeForManageTemporaryStorageCancelationWhenProcessResponse(guarantee, tranValue: -4.0m);
				});
			}
		}

		void AssertTransactionForManageTemporaryStorageCancelationWhenProcessResponse(ICusTempStorageRegLine regLine, ZDecimal bondAmound, int packageQty, decimal grossWeight)
		{
			var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_Comments.Contains("(Canceled)"));

			AssertEquals("SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
			AssertEquals("SRT_InternalReferenceType", "DUA", transaction.SRT_InternalReferenceType);
			AssertEquals("SRT_InternalReferenceNumber", "TestReference", transaction.SRT_InternalReferenceNumber);
			AssertEquals("SRT_Comments", "TPref: CommentReference (Canceled)", transaction.SRT_Comments);
			AssertEquals("SRT_TransactionDate", ZDateTime.BrettsBirthday.ToDateTimeOffset(null), transaction.SRT_TransactionDate);
			AssertEquals("SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
			AssertEquals("SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
			AssertEquals("SRT_BondAmount", bondAmound, transaction.SRT_BondAmount);
			AssertEquals("SRT_Reference", MRNCode, transaction.SRT_Reference);
		}

		void AssertGuaranteeForManageTemporaryStorageCancelationWhenProcessResponse(CusGuaranteeHeader cusGuarantee, ZDecimal tranValue)
		{
			var guarantee = cusGuarantee.CusGuaranteeLineTransactions.First(x => x.CPL_Comment.Contains("(Canceled)") && x.CPL_TranValue == tranValue);

			AssertEquals("CPL_TransactionType", "TRA", guarantee.CPL_TransactionType);
			AssertEquals("CPL_TransactionDate", ZDateTime.BrettsBirthday, guarantee.CPL_TransactionDate);
			AssertEquals("CPL_Reference", RegHeaderReference, guarantee.CPL_Reference);
			AssertEquals("CPL_TranValue", tranValue, guarantee.CPL_TranValue);
			AssertEquals("CPL_Comment", "GPref TestReference. MRN: " + MRNCode + " (Canceled)", guarantee.CPL_Comment);
			AssertEquals("CPL_TransactionStatus", "CON", guarantee.CPL_TransactionStatus);
		}

		#endregion

		#region GetDataToReserveTemporaryStorageGoods

		public void TestGetDataToReserveTemporaryStorageGoods_WithErrorFromGetDeclarationDataToReserve()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_ReturnsError);

				AssertEquals("resultMessage is not empty", "Error Getting Data", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithoutExpectedDoc()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithoutRegHeader()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: "reference");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565789456123789 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565789456123789AAA are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, jobNumber: "JOB001");

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565789456123789 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565789456123789AAA are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565789456123789. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565789456123789AAA. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789AAA, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789AAA, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 1.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, resultMessageVin, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("resultMessage is not empty", "ES00001:\nGross weight 0 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 1234565, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVin);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, regHeaderReference: PrevDocReferenceShort);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, resultMessageVin, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("resultMessage is not empty", "ES00001:\nGross weight 0 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 1234565, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVin);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, regHeaderReference: PrevDocReferenceLong);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, resultMessageVin, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("resultMessage is not empty", "ES00001:\nGross weight 0 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 1234565789456123789, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVin);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, resultMessageVin, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("resultMessage is not empty", "ES00001:\nGross weight 0 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 1234565789456123789AAA, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVin);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutGrossWeightForVINsErrorWithDecimalsInOBL()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 0.7m);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, resultMessageVin, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("resultMessage is empty", ZString.Empty, resultMessageVin);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutGrossWeightForVINsErrorWithDecimalsInGrossWeight()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 20m);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, resultMessageVin, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18_WithTotalGrossWeightForVINs);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("resultMessage is empty", ZString.Empty, resultMessageVin);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReferenceShort);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReferenceLong);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReferenceLong + "AAA");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_HavingFormatDocRef_DocRefShorterThan18()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, regHeaderReference: PrevDocReferenceShort);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var secondDocRef = "refdoc1234567891234";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, regHeaderReference: PrevDocReferenceLong);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 1.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc1234567891234, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var secondDocRef = "refdoc1234567891234AAA";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc1234567891234AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		#endregion

		#region GetDataToReserveTemporaryStorageGoods Using CSI_ItemNumber

		public void TestGetDataToReserveTemporaryStorageGoods_WithErrorFromGetDeclarationDataToReserve_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_ReturnsError, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "Error Getting Data", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithoutExpectedDoc_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithoutRegHeader_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: "reference", goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565789456123789 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565789456123789AAA are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingJobNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, jobNumber: "JOB001");

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingJobNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingJobNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565789456123789 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_UsingCSI_ItemNumber_HavingJobNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565789456123789AAA are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 1);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 1, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 1, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 1234565789456123789. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 1, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 1234565789456123789AAA. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789AAA, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789AAA, Item 2. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, goodsItemNumber: 2);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 2: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 2: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 2: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 2: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 2: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, goodsItemNumber: 2);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 2.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 2.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 2.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 2.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_UsingCSI_ItemNumber()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_UsingCSI_ItemNumber()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, goodsItemNumber: 2);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, useCSI_ItemNumber: true);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 2.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 2.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var secondDocRef = "refdoc1234567891234";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 2.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc1234567891234, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_UsingCSI_ItemNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var secondDocRef = "refdoc1234567891234AAA";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18, useCSI_ItemNumber: true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 2.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc1234567891234AAA, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		#endregion

		#region GetDataToReserveTemporaryStorageGoods Amendment

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithErrorFromGetDeclarationDataToReserve()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_ReturnsError, true);

				AssertEquals("resultMessage is not empty", "Error Getting Data", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithoutExpectedDoc()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithoutRegHeader()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: "reference");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565789456123789 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 1234565789456123789AAA are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, jobNumber: "JOB001");

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565789456123789 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, jobNumber: "JOB001", formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "JOB001/ES00001: Goods in TSD Number 1234565789456123789AAA are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565789456123789. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565789456123789AAA. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 1234565789456123789AAA, Item 1. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutVINError_WhenCONTransactionExists_WithLineStatusCLS()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutVINError_WhenCONTransactionExists_WithLineStatusCLS_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", regHeaderReference: PrevDocReferenceShort);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutVINError_WhenCONTransactionExists_WithLineStatusCLS_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", regHeaderReference: PrevDocReferenceLong);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutVINError_WhenCONTransactionExists_WithLineStatusCLS_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS", regHeaderReference: PrevDocReferenceLong + "AAA");

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutPackageError_NotBulk_WhenCONTransactionExists_NoDifferences()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutPackageError_NotBulk_WhenCONTransactionExists_NoDifferences_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 1: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 1234565789456123789AAA, Item 1: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 3.06m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 8;
			regLineTransaction3.SRT_GrossWeight = 27.2m;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_HavingFormatDocRef_DocRefShorterThan18()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 3.06m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_GrossWeight = 29.6m;

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, false, regLine3),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_GetDataToReserve_WithoutDifferences()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 3.06m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 8;
			regLineTransaction3.SRT_GrossWeight = 27.2m;

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 20m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_GetDataToReserve_WithDifferences()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
				(20m, 1, 1, true, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, true);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 7\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_GetDataToReserve_WithDifferences_HavingFormatDocRef_DocRefShorterThan18()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, regHeaderReference: PrevDocReferenceShort);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
				(20m, 1, 1, true, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565, Item 1.\nRemaining Gross Weight in the Temporary Storage: 7\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_GetDataToReserve_WithDifferences_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var secondDocRef = "refdoc1234567891234";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, regHeaderReference: PrevDocReferenceLong);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
				(20m, 1, 1, true, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789, Item 1.\nRemaining Gross Weight in the Temporary Storage: 7\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc1234567891234, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_GetDataToReserve_WithDifferences_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var secondDocRef = "refdoc1234567891234AAA";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m, regHeaderReference: PrevDocReferenceLong + "AAA");

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
				(20m, 1, 1, true, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18, true, formatDocRef: FormatDocRefForTest);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for TSD Number 1234565789456123789AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 7\n\n" +
					"There might not be enough Gross Weight 20 for TSD Number refdoc1234567891234AAA, Item 1.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 3.06m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 8;
			regLineTransaction3.SRT_GrossWeight = 27.2m;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction2.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = 1;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "FR";
			regLine4.SRL_PackageMarks = "AAAAAA";
			regLine4.SRL_SRH = regLine1.RegHeader.PK;
			var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction2.SRT_GrossWeight = 10;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 8, 9, true, regLine3),
				(30.6m, 0, 9, true, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

				var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
				AssertNotNull("regLine4 has new transaction", transaction);
				AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
				AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others, transaction.SRT_InternalReferenceType);
				AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
				AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
				AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
			});
		}

		#endregion

		#region GetDataToReserveTemporaryStorageGoods LAME

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithErrorFromGetDeclarationDataToReserve()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_ReturnsError, isLAME: true);

				AssertEquals("resultMessage is not empty", "Error Getting Data", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithoutExpectedDoc()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, "AAA", LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithoutRegHeader()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: "reference");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is no record in the Temporary Storage Register for LAME Reception Certificate 1234565. You might have mistaken the number.\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithOutRegHeaderWithoutPremises()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");
			regLineTransaction.RegLine.RegHeader.SRH_Reference = "AAAA";

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is no record in the Temporary Storage Register for LAME Reception Certificate 1234565. You might have mistaken the number.\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithoutPremises()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in LAME Reception Certificate 1234565 are not stored in location 9999000002. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithOutRegHeaderWithoutPremises_HavingJobNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");
			regLineTransaction.RegLine.RegHeader.SRH_Reference = "AAAA";

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, jobNumber: "JOB001", isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is no record in the Temporary Storage Register for LAME Reception Certificate 1234565. You might have mistaken the number.\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithoutPremises_HavingJobNumber()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, jobNumber: "JOB001", isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: Goods in LAME Reception Certificate 1234565 are not stored in location 9999000002. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(goodsItemNumber: 2);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithVINError()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the LAME under Reception Certificate Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithVINError_WhenCONTransactionExists_WithLineStatusCLS()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regLineCustomsStatus: "CLS");

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the LAME under Reception Certificate Number 1234565. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 1234565: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WithPackagesInSupportingDocuments()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18_WithPackagesInSupportingDocuments, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 1234565: 3 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_NoDifferences()
		{
			var (regLineTransaction, _, _, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 8, transactionGrossWeight: 5m);

			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = -1;

			Factory.Save();

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 1234565: 8 BX. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk()
		{
			var (regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageType: "V0", transactionGrossWeight: 5m);

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 1234565: 0 VQ. Please, correct data and send again.", resultMessage);
				AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 30.6 for Reception Certificate Number 1234565.\nRemaining Gross Weight in the Temporary Storage: 11\n\nDo you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises()
		{
			var (regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoods_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		public void TestGetDataToReserveTemporaryStorageGoods_IsLAME_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs()
		{
			var secondDocRef = "refdoc";

			var (regLineTransaction, regLine1, regLine2, regLine3, premises, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 6m);

			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = "BBB";
			regHeader.SRH_Reference = secondDocRef;
			regHeader.SRH_SRP_Premises = premises.PK;

			var regLine4 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 4;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VQ";
			regLine4.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction4.SRT_GrossWeight = 6m;

			var regLineItem2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 1;

			var regLineItemPivot4 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

			Factory.Save();

			var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, ICusTempStorageRegLine)>()
			{
				(30.6m, 1, 9, false, regLine1),
				(30.6m, 8, 9, false, regLine3),
				(30.6m, 0, 9, false, regLine2),
				(20m, 1, 1, false, regLine4),
			};

			CombineAssertions(() =>
			{
				var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, InternalRefNum, InternalRefType, EntryReference, PrevDocCode, LocationInEntry, GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18, isLAME: true);

				AssertEquals("resultMessage is not empty",
					"ES00001:\nThere might not be enough Gross Weight 30.6 for Reception Certificate Number 1234565.\nRemaining Gross Weight in the Temporary Storage: 12\n\n" +
					"There might not be enough Gross Weight 20 for Reception Certificate Number refdoc.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
					"Do you want to cancel this declaration to check?", resultMessage);
				AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, d.RegLine)));
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}

		#endregion

		void AssertConfirmedTransaction(ZString transactionName, ICusTempStorageRegLineTransaction transaction, ZDecimal expectedBondAmount, string expectedOldComment = "")
		{
			AssertEquals(transactionName + "'s SRT_TransactionStatus was changed", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
			AssertEquals(transactionName + "'s SRT_ReferenceType was changed", "MRN", transaction.SRT_ReferenceType);
			AssertEquals(transactionName + "'s SRT_Reference was changed", MRNCode, transaction.SRT_Reference);
			AssertEquals(transactionName + "'s SRT_Comments was changed", ExpectedComment + expectedOldComment, transaction.SRT_Comments);
			AssertEquals(transactionName + "'s SRT_TransactionDate was changed", issueDate.ToOffset(), transaction.SRT_TransactionDate);
			AssertEquals(transactionName + "'s SRT_PhysicalInOutDate was changed", releaseDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
			AssertEquals(transactionName + "'s SRT_BondAmount was changed", expectedBondAmount, transaction.SRT_BondAmount);
		}

		void AssertNewTransactionToReserveGoods(ICusTempStorageRegLine regLine, ZString internalRefNum, ZString internalRefType, ZInt expectedPackQty, ZDecimal expectedGrossWeight, string expectedComment = "")
		{
			var lineNum = regLine.SRL_LineNumber;
			AssertEquals("Line " + lineNum + " has status OPN", "OPN", regLine.SRL_CustomsStatus);

			var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_GrossWeight == expectedGrossWeight);
			AssertNotNull("Line " + lineNum + " has new transaction", transaction);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceNumber correct", internalRefNum, transaction.SRT_InternalReferenceNumber);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceType correct", internalRefType, transaction.SRT_InternalReferenceType);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_PackageQty correct", expectedPackQty, transaction.SRT_PackageQty);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_GrossWeight correct", expectedGrossWeight, transaction.SRT_GrossWeight);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_Comments correct", expectedComment, transaction.SRT_Comments);
		}

		void AssertNewTransactionToRestoreVehicle(ICusTempStorageRegLine regLine, ZString internalRefNum, ZString internalRefType, ZDecimal expectedGrossWeight)
		{
			var lineNum = regLine.SRL_LineNumber;
			AssertEquals("Line " + lineNum + " has status OPN", "OPN", regLine.SRL_CustomsStatus);

			var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("Line " + lineNum + " has new transaction", transaction);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceNumber correct", internalRefNum, transaction.SRT_InternalReferenceNumber);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceType correct", internalRefType, transaction.SRT_InternalReferenceType);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_GrossWeight correct", expectedGrossWeight, transaction.SRT_GrossWeight);
			AssertEquals("Line " + lineNum + " has new transaction with SRT_Comments correct", AdjustmentTransactionComment, transaction.SRT_Comments);
		}

		void AssertWriteOffTransaction(BaseCusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue, ZDateTime expectedTransactionDate, ZString expectedComment)
		{
			AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
			AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
			AssertEquals(transactionName + "'s CPL_TransactionDate", expectedTransactionDate, transaction.CPL_TransactionDate);
			AssertEquals(transactionName + "'s CPL_Comment", expectedComment, transaction.CPL_Comment);
			AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
			AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
		}

		public void TestDataToUpdateLocationAndReference()
		{
			var dataToUpdateLocationAndReference = new DataToUpdateLocationAndReference()
			{
				Location = "NewLocation",
				EmptyLocation = ZBool.False,
				Reference = ZString.Empty,
				EmptyReference = ZBool.True,
			};

			CombineAssertions(() =>
			{
				AssertEquals("Expected Location assigned value", "NewLocation", dataToUpdateLocationAndReference.Location);
				AssertEquals("Expected EmptyLocation assigned value", ZBool.False, dataToUpdateLocationAndReference.EmptyLocation);
				AssertEquals("Expected Reference assigned value", ZString.Empty, dataToUpdateLocationAndReference.Reference);
				AssertEquals("Expected EmptyReference assigned value", ZBool.True, dataToUpdateLocationAndReference.EmptyReference);
			});
		}

		const string RegHeaderReference = "reference";
		const string InternalRefNum = "ES00001";
		const string InternalRefType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		const string MRNCode = "20ES00999930006184";
		const string CommentPrefix = "Prefix";
		const string JobNumberForComment = "B00000001";
		const string ExpectedComment = CommentPrefix + " " + JobNumberForComment;
		const string OldComment = "Extra Old Comment";
		const string AdjustmentTransactionComment = "Adjustment for Complementary Declaration";
		const string WriteOffComment = " / DUA";
		readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);
		readonly ZDateTime releaseDate = new ZDateTime(2024, 06, 14, 11, 11, 11);

		const string EntryReference = "ES00001";
		const string PrevDocCode = "SUM";
		const string PrevDocReferenceShort = "1234565";
		const string PrevDocReferenceLong = "1234565789456123789";
		const string LocationInEntry = "9999000002";

		ZString[] CustomsStatusToCancelTemporaryStoragePendingTransactions => new ZString[] { "AAA" };
		ZString[] CustomsStatusToConfirmTemporaryStoragePendingTransactions => new ZString[] { "BBB" };
		ZString[] CustomsStatusToNotCreateTemporaryStorageTransactions => new ZString[] { "CCC", ZString.Empty };

		ZString FormatDocRefForTest(ZString docRef) => docRef + "AAA";

		ICusTempStorageRegLineTransaction SetUpTransaction(ICusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction, decimal bondAmount = 0.0m)
		{
			var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = transactionType;
			regLineTransaction.SRT_TransactionStatus = transactionStatus;
			regLineTransaction.SRT_InternalReferenceType = InternalRefType;
			regLineTransaction.SRT_PackageQty = packageQty;
			regLineTransaction.SRT_GrossWeight = grossWeight;
			regLineTransaction.SRT_BondAmount = bondAmount;

			if (transactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance)
			{
				regLineTransaction.SRT_InternalReferenceNumber = InternalRefNum;
			}

			return regLineTransaction;
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		}

		EUInterfaces.ICusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = RegHeaderReference)
		{
			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = appCode;
			regHeader.SRH_Reference = reference;

			return regHeader;
		}

		CusGuaranteeHeader SetUpGuaranteeForTempStorage(EUInterfaces.ICusTempStorageRegHeader regHeader, ZDecimal value)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var cusGuarantee = Factory.New<CusGuaranteeHeader>();
			cusGuarantee.CPH_Number = "Test1";
			cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
			cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
			cusGuarantee.CPH_SubType = "1";
			cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
			cusGuarantee.CPH_Balance = 1000.0m;

			var commonGuarantee = Factory.New<CommonGuarantee>();
			commonGuarantee.Parent = (BusinessObject)regHeader;
			commonGuarantee.PW_BondNumber = "Test1";
			commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

			var guarantee = ((CommonGuarantee)(regHeader.Guarantee)).CusGuarantee;
			var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = RegHeaderReference;
			guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			guaranteeLineTransaction.CPL_TranValue = value;
			guaranteeLineTransaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;

			guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			return guarantee;
		}

		ICusTempStorageRegLine SetUpRegLine(ICusTempStorageRegHeader regHeader)
		{
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_PackageType = "BX";

			return regLine;
		}

		(ICusTempStorageRegLineTransaction regLineTransaction, ICusTempStorageRegLine regLine, ICusTempStorageRegHeader regHeader) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
			(string locationInPremises = LocationInEntry, string regHeaderReference = PrevDocReferenceShort, bool createTransaction = true, bool isLAME = false)
		{
			var (_, orgAddress) = SetUpOrgHeader();

			var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises.SRP_Type = isLAME ? "LAM" : "ADT";
			premises.SRP_CustomsLocation = locationInPremises;
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
			regHeader.SRH_SRP_Premises = premises.PK;
			var regLine = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_CustomsStatus = "OPN";
			regLine.SRL_PackageType = "VQ";
			regLine.SRL_SRH = regHeader.PK;
			var regLineTransaction = (ICusTempStorageRegLineTransaction)null;
			if (createTransaction)
			{
				regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
				regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
				regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
				regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
				regLineTransaction.SRT_SRL = regLine.PK;
			}

			var regLineItem = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem.SRI_GoodsItemNumber = 1;

			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine.PK;
			Factory.Save();

			return (regLineTransaction, regLine, regHeader);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoods_ReturnsError() => (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), "Error Getting Data");

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoods_DocRefShorterThan18_WithPackagesInSupportingDocuments()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceShort;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			doc.CSI_PackQty = 3;
			doc.CSI_PackType = "BX";
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("FR", 1, "VIN1", false),
						("BX", 8, "", false),
						("VQ", 0, "", true),
					},
					TotalGrossWeight = 30.6m
				}
			};

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoods_DocRefShorterThan18_WithTotalGrossWeightForVINs()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceShort;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("FR", 1, "VIN1", false),
						("BX", 8, "", false),
						("VQ", 0, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 20.555m
				}
			};

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoods_DocRefShorterThan18()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceShort;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("FR", 1, "VIN1", false),
						("BX", 8, "", false),
						("VQ", 0, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 0
				}
			};

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoods_DocRefLongerThan18()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceLong;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("FR", 1, "VIN1", false),
						("BX", 8, "", false),
						("VQ", 0, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 0
				}
			};

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefShorterThan18()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceShort;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("FR", 1, "VIN1", false),
						("BX", 8, "", false),
						("VQ", 0, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 0
				}
			};
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_Code = PrevDocCode;
			doc2.CSI_ReferenceNumber = "refdoc";
			doc2.CSI_LineNo = 1;
			doc2.CSI_ItemNumber = 2;
			dataToReturn.Add(
				new DeclarationDataToReserveTSGoods()
				{
					Document = doc2,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
									("VQ", 1, "", true),
					},
					TotalGrossWeight = 20m,
					TotalGrossWeightForVINs = 0
				});

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoodsWith2Docs_DocRefLongerThan18()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceLong;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("FR", 1, "VIN1", false),
						("BX", 8, "", false),
						("VQ", 0, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 0
				}
			};
			var doc2 = Factory.New<CusSupportingInfo>();
			doc2.CSI_Code = PrevDocCode;
			doc2.CSI_ReferenceNumber = "refdoc1234567891234";
			doc2.CSI_LineNo = 1;
			doc2.CSI_ItemNumber = 2;
			dataToReturn.Add(
				new DeclarationDataToReserveTSGoods()
				{
					Document = doc2,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
									("VQ", 1, "", true),
					},
					TotalGrossWeight = 20m,
					TotalGrossWeightForVINs = 0
				});

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefShorterThan18()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceShort;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("VQ", 1, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 0
				}
			};

			return (dataToReturn, ZString.Empty);
		}

		(IEnumerable<DeclarationDataToReserveTSGoods>, ZString) GetDeclarationDataToReserveTSGoodsWithOnlyOnePackage_DocRefLongerThan18()
		{
			var doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = PrevDocCode;
			doc.CSI_ReferenceNumber = PrevDocReferenceLong;
			doc.CSI_LineNo = 1;
			doc.CSI_ItemNumber = 2;
			var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
			{
				new ()
				{
					Document = doc,
					Packages = new (ZString, ZInt, ZString, ZBool)[]
					{
						("VQ", 1, "", true),
					},
					TotalGrossWeight = 30.6m,
					TotalGrossWeightForVINs = 0
				}
			};

			return (dataToReturn, ZString.Empty);
		}

		(OrgHeader orgHeader, OrgAddress orgAddress) SetUpOrgHeader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			return (orgHeader, orgAddress);
		}

		(ICusTempStorageRegLineTransaction regLineTransaction, ICusTempStorageRegLine regLine1, ICusTempStorageRegLine regLine2, ICusTempStorageRegLine regLine3, ICusTempStorageRegPremises premises, ICusTempStorageRegLineItem regLineItem) SetUpDataForGetDataToReserveTemporaryStorageGoods
			(int goodsItemNumber = 1, string locationInPremises = LocationInEntry,
			string regHeaderReference = PrevDocReferenceShort, string packageVin = "VIN11", int packageQtyNotBulk = 10, decimal transactionGrossWeight = 40m, decimal transactionGrossWeightOBLForVINs = 5m, bool isForAmendment = false, string regLineCustomsStatus = "OPN", string bulkPackageType = "VQ")
		{
			var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = locationInPremises;
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			var (orgHeader, orgAddress) = SetUpOrgHeader();
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
			regHeader.SRH_SRP_Premises = premises.PK;
			var regLine1 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine1.SRL_LineNumber = 2;
			regLine1.SRL_CustomsStatus = regLineCustomsStatus;
			regLine1.SRL_PackageType = isForAmendment ? "CT" : "FR";
			regLine1.SRL_PackageMarks = packageVin;
			regLine1.SRL_SRH = regHeader.PK;
			var regLineTransactionPND = regLine1.CusTempStorageRegLineTransactions.AddNew();
			regLineTransactionPND.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransactionPND.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			regLineTransactionPND.SRT_InternalReferenceNumber = EntryReference;
			regLineTransactionPND.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction1.SRT_PackageQty = 10;
			regLineTransaction1.SRT_GrossWeight = transactionGrossWeightOBLForVINs;

			var regLine2 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine2.SRL_LineNumber = 3;
			regLine2.SRL_CustomsStatus = "OPN";
			regLine2.SRL_PackageType = bulkPackageType;
			regLine2.SRL_SRH = regHeader.PK;
			var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction2.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction2.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

			var regLine3 = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine3.SRL_LineNumber = 4;
			regLine3.SRL_CustomsStatus = "OPN";
			regLine3.SRL_PackageType = "BX";
			regLine3.SRL_SRH = regHeader.PK;
			var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
			regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;
			regLineTransaction3.SRT_GrossWeight = 1;

			var regLineItem = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem.SRI_GoodsItemNumber = goodsItemNumber;

			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

			var regLineItemPivot2 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

			var regLineItemPivot3 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

			Factory.Save();

			return (regLineTransactionPND, regLine1, regLine2, regLine3, premises, regLineItem);
		}

		(ICusTempStorageRegLineTransaction regLineTransaction, ICusTempStorageRegLine regLine, EUInterfaces.ICusTempStorageRegHeader regHeader) SetUpDataForManageTemporaryStorageCancelationWhenProcessResponse(bool createTransaction = true, string transactionType = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, string premiseType = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse)
		{
			var (orgHeader, orgAddress) = SetUpOrgHeader();

			var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises.SRP_Type = premiseType;
			premises.SRP_CustomsLocation = LocationInEntry;
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var regHeader = SetUpTmpRegHeader();
			regHeader.SRH_SRP_Premises = premises.PK;
			regHeader.SRH_Status = "CLS";

			var cusGuarantee = Factory.New<CusGuaranteeHeader>();
			cusGuarantee.CPH_Number = "Test1";
			cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
			cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
			cusGuarantee.CPH_SubType = "1";
			cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;

			var commonGuarantee = Factory.New<CommonGuarantee>();
			commonGuarantee.Parent = (BusinessObject)regHeader;
			commonGuarantee.PW_BondNumber = "Test1";
			commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

			var guarantee = ((CommonGuarantee)regHeader.Guarantee).CusGuarantee;
			var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = "reference";
			guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			guaranteeLineTransaction.CPL_TranValue = 2.0m;

			var regLine = SetUpRegLineForManageTemporaryStorageCancelationWhenProcessResponse(regHeader, 1);
			var regLineTransaction = createTransaction ? SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(regLine, transactionType) : Factory.New<ICusTempStorageRegLineTransaction>();

			return (regLineTransaction, regLine, regHeader);
		}

		ICusTempStorageRegLine SetUpRegLineForManageTemporaryStorageCancelationWhenProcessResponse(ICusTempStorageRegHeader regHeader, ZInt lineNumber)
		{
			var regLine = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine.SRL_LineNumber = lineNumber;
			regLine.SRL_CustomsStatus = "CLS";
			regLine.SRL_SRH = regHeader.PK;

			return regLine;
		}

		ICusTempStorageRegLineTransaction SetUpTransactionForManageTemporaryStorageCancelationWhenProcessResponse(ICusTempStorageRegLine regLine, string status = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, string referenceNum = "TestReference", string referenceType = "DUA")
		{
			var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction1.SRT_TransactionStatus = status;
			regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
			regLineTransaction1.SRT_InternalReferenceType = referenceType;
			regLineTransaction1.SRT_Reference = MRNCode;

			return regLineTransaction1;
		}
	}
}
