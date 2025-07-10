using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSPackage))]
	class EMCSPackageTest : CusInvPackTest<EMCSJobDeclaration>
	{
		public void TestIsMessageStatusSendOrAwaitOnParent()
		{
			var messageForReadOnly = @"Should only be readonly and can not delete with these two conditions:
		a)The package links to a EMCS invoice line and EMCS Declaration.
		c)The message stauts of parent declaration is SNT or ACK.";

			void AssertCanDeleteAndReadOnly(EMCSPackage pack, bool expectedReadOnly)
			{
				AssertEquals(messageForReadOnly, !expectedReadOnly, pack.CanDelete);
				AssertEquals(messageForReadOnly, expectedReadOnly, pack.ReadOnly);
			}

			var defaultPack = Factory.New<EMCSPackage>();
			AssertCanDeleteAndReadOnly(defaultPack, false);

			var packWithInvoiceLine = declaration.EMCSPackages.AddNew();

			declaration.JE_MessageStatus = ZString.Empty;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(packWithInvoiceLine, false);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(packWithInvoiceLine, true);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(packWithInvoiceLine, true);
		}

		public void TestLookups()
		{
			AssertType<EMCSPackageLookups>(package.Lookups);
		}

		public void TestValidation()
		{
			AssertType<EMCSPackageValidation>(package.Validation);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(0, package.B5_UnitCount);
		}

		public void TestDelete()
		{
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var packagePivot1 = invoiceLine1.EMCSPackagePivots[0];
			packagePivot1.IsForInvoiceLine = true;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var packagePivot2 = invoiceLine2.EMCSPackagePivots[0];
			packagePivot2.IsForInvoiceLine = true;
			CombineAssertions(() =>
			{
				AssertEquals("Multiple Gen Pivot records", 2, LoadGenPivot(package).Length);
				package.Delete();
				AssertEquals("Pivots deleted", 0, LoadGenPivot(package).Length);
			});

			GenPivot[] LoadGenPivot(EMCSPackage package)
			{
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, "EMC");
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, package.PK);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusInvPackSchema.Constants.Prefix);
				return Factory.Load<GenPivot>(pivotQuery);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			declaration = factory.New<EMCSJobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			package = declaration.EMCSPackages.AddNew();
			return package;
		}

		void CreateCountableUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");

			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);

			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected override void SetUp()
		{
			CreateCountableUQ();
			base.SetUp();
			GetNewBusinessObject();
		}
		EMCSPackage package;
		EMCSJobComInvoiceHeader invoice;
		EMCSJobDeclaration declaration;
	}
}
