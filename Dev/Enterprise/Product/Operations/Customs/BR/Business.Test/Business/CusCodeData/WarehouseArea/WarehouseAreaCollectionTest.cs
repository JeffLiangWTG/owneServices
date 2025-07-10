using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(WarehouseAreaCollection))]
	public class WarehouseAreaCollectionTest : CusCodeDataCollectionTest<WarehouseArea>
	{
		protected override CusCodeDataCollection<WarehouseArea> GetCusCodeDataCollection()
		{
			return new WarehouseAreaCollection(Declaration);
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
