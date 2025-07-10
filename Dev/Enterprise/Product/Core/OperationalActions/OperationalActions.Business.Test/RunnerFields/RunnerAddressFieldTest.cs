using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerAddressField))]
	internal sealed class RunnerAddressFieldTest : RunnerFieldTest<RunnerAddressField, OperationalActionAddressFieldSupporter, ZGuid>
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerAddressField(Factory, Descriptor, NewSupporter());
		}

		protected override OperationalActionAddressFieldSupporter NewSupporter()
		{
			return new OperationalActionAddressFieldSupporter(ColumnOnDummy.Name, false, AddressType.OFC, delegate(BusinessObjectFactory factory)
			{
				return new ConsignorCollection(factory);
			});
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Guid;
			}
		}

		protected override ZGuid EmptyValue
		{
			get
			{
				return ZGuid.Empty;
			}
		}

		protected override ZGuid Value1
		{
			get
			{
				return Address.PK;
			}
		}

		OrgAddress Address
		{
			get
			{
				return address ?? (address = Factory.LoadTop1<OrgAddress>(new ZQuery()));
			}
		}

		OrgAddress address;
		#endregion
	}
}
