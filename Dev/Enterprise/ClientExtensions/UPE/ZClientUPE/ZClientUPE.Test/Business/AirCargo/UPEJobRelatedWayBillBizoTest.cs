using System.Collections.Specialized;
using System.Reflection;
using CargoWise.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.AirCargo.Testing
{
	[TestedType(typeof(UPEJobRelatedWayBill))]
	internal class UPEJobRelatedWayBillBizoTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEJobDeclaration>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestValidation()
		{
			UPEJobRelatedWayBill childPackage = Factory.New<UPEJobRelatedWayBill>();
			AssertEquals(typeof(UPEJobRelatedWayBillValidation), childPackage.Validation.GetType());
		}

		public void TestNoDuplicateConstants()
		{
			FieldInfo[] fields = typeof(Customs.AU.Declaration.Business.JobRelatedWayBill.Constants.RelatedWayBillType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			StringDictionary deDuplicatedFieldsList = new StringDictionary();
			foreach (FieldInfo info in fields)
			{
				if (deDuplicatedFieldsList[info.GetRawConstantValue().ToString()] == null)
				{
					deDuplicatedFieldsList.Add(info.GetRawConstantValue().ToString(), null);
				}
			}

			AssertEquals(fields.Length, deDuplicatedFieldsList.Count);
		}
	}
}
