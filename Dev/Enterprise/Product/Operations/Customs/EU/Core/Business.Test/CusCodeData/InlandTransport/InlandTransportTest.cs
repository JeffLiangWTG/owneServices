using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InlandTransport))]
	sealed class InlandTransportTest : Customs.Business.Testing.CusCodeDataTest<InlandTransport>
	{
		[ExpectNoExceptions]
		public void TestCY_Code_MaxLength()
		{
			NUnit.Framework.Assert.That(inlandTransport.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestCY_Code_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(inlandTransport.CY_CodeInfo, null).Caption, NUnit.Framework.Is.EqualTo("Type of ID"));
		}

		[ExpectNoExceptions]
		public void TestCY_Data_MaxLength()
		{
			NUnit.Framework.Assert.That(inlandTransport.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(35));
		}

		[ExpectNoExceptions]
		public void TestCY_Data_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(inlandTransport.CY_DataInfo).Caption, NUnit.Framework.Is.EqualTo("Transport ID"));
		}

		[ExpectNoExceptions]
		public void TestNationality_MaxLength()
		{
			NUnit.Framework.Assert.That(inlandTransport.NationalityInfo.MaxLength, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestNationality_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(inlandTransport.NationalityInfo).Caption, NUnit.Framework.Is.EqualTo("Nationality"));
		}

		[ExpectNoExceptions]
		public void TestNationalityGetter()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(inlandTransport.Nationality, NUnit.Framework.Is.EqualTo(ZString.Empty), "Nationality empty by default");

				GenAddOnHelper.FindOrMakeNewAddOn(InlandTransport.Schema.Nationality, inlandTransport, out var nationalityColumn);
				nationalityColumn.XA_Data = Core.Constants.CountryCodes.Australia;
				NUnit.Framework.Assert.That(inlandTransport.Nationality, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Australia).Using(CustomComparers.TypeComparison), "Nationality 'AU'");

				inlandTransport.Nationality = ZString.Empty;
				NUnit.Framework.Assert.That(inlandTransport.Nationality, NUnit.Framework.Is.EqualTo(ZString.Empty), "Nationality after GenAddOnColumn deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestNationalitySetter()
		{
			GenAddOnColumn nationalityColumn;
			CombineAssertions(() =>
			{
				inlandTransport.Nationality = Core.Constants.CountryCodes.Germany;
				GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
				NUnit.Framework.Assert.That(nationalityColumn.XA_Data, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Germany).Using(CustomComparers.TypeComparison), "Nationality set to 'DE'");

				inlandTransport.Nationality = Core.Constants.CountryCodes.Australia;
				GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
				NUnit.Framework.Assert.That(nationalityColumn.XA_Data, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Australia).Using(CustomComparers.TypeComparison), "Nationality set to 'AU'");

				inlandTransport.Nationality = ZString.Empty;
				GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
				NUnit.Framework.Assert.That(nationalityColumn, NUnit.Framework.Is.EqualTo(default(GenAddOnColumn)), "Nationality set to empty, GenAddOnColumn deleted - should be [null]");

				inlandTransport.Nationality = Core.Constants.CountryCodes.Italy;
				GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
				NUnit.Framework.Assert.That(nationalityColumn.XA_Data, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Italy).Using(CustomComparers.TypeComparison), "Nationality set to 'IT' after GenAddOnColumn deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestGenAddOnDeletedOnDelete()
		{
			inlandTransport.Nationality = Core.Constants.CountryCodes.Germany;
			inlandTransport.Delete();

			GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out var addOn);
			NUnit.Framework.Assert.That(addOn, NUnit.Framework.Is.EqualTo(default(GenAddOnColumn)));
		}

		[ExpectNoExceptions]
		public void TestCY_Order()
		{
			var inlandTransport2 = declaration.InlandTransports.AddNew();
			var inlandTransport3 = declaration.InlandTransports.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(inlandTransport.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1), "Order 1");
				NUnit.Framework.Assert.That(inlandTransport2.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2), "Order 2");
				NUnit.Framework.Assert.That(inlandTransport3.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)3), "Order 3");

				declaration.InlandTransports.Remove(inlandTransport2);
				NUnit.Framework.Assert.That(inlandTransport.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1), "Order 1 stay same");
				NUnit.Framework.Assert.That(inlandTransport3.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2), "Order 3 change to 2");
			});
		}

		[ExpectNoExceptions]
		public void TestISequenceNumberLine()
		{
			CombineAssertions(() =>
			{
				var sequenceLine = (IShortSequenceNumberLine)inlandTransport;
				NUnit.Framework.Assert.That(sequenceLine.FKToHeader, NUnit.Framework.Is.EqualTo(declaration.PK), "FKToHeader");
				NUnit.Framework.Assert.That(sequenceLine.SequenceNumber, NUnit.Framework.Is.EqualTo((ZShort)1), "SequenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var transport = Factory.New<InlandTransport>();
			NUnit.Framework.Assert.That(transport.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.TransportInland).Using(CustomComparers.TypeComparison), "CY_Type");
		}

		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			NUnit.Framework.Assert.That(inlandTransport.HumanReadableName, NUnit.Framework.Is.EqualTo("Inland Transport").Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().InlandTransports.AddNew();

		protected override IEnumerable<InlandTransport> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (InlandTransport)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			return declaration.InlandTransports.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			inlandTransport = declaration.InlandTransports.AddNew();
		}
		InlandTransport inlandTransport;
		JobDeclaration declaration;
	}
}
