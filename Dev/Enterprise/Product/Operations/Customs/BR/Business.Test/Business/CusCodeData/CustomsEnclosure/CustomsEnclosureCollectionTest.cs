using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CustomsEnclosureCollection))]
	public class CustomsEnclosureCollectionTest : CusCodeDataCollectionTest<CustomsEnclosure>
	{
		protected override CusCodeDataCollection<CustomsEnclosure> GetCusCodeDataCollection()
		{
			return new CustomsEnclosureCollection(Declaration);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
