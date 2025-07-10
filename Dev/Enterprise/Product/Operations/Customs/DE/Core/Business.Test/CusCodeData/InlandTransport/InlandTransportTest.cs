using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InlandTransport))]
	sealed class InlandTransportTest : Customs.Business.Testing.CusCodeDataTest<InlandTransport>
	{
		[ExpectNoExceptions]
		public void TestCY_Data_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(inlandTransport.CY_DataInfo).Caption, NUnit.Framework.Is.EqualTo("Wagon Number"));
		}

		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			NUnit.Framework.Assert.That(inlandTransport.HumanReadableName, NUnit.Framework.Is.EqualTo("Wagon Number").Using(CustomComparers.TypeComparison));
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
