using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedBooleanFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionBooleanFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedBooleanFieldDefaultingStrategy(OperationalActionBooleanFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("6f42dfb0-a366-4af6-91d7-5ea8fe847ca5", "Fixed Boolean"), fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.TextDropEdit; }
		}

		public override int DetailMaxLength
		{
			get { return 3; }
		}

		public override IZType GetDefaultValue(string detail)
		{
			return (ZString)detail;
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public override IList GetBoundCollection(BusinessObjectFactory factory)
		{
			return new BooleanChangeType();
		}
	}
}
