using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedCodeFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionCodeFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedCodeFieldDefaultingStrategy(OperationalActionCodeFieldSupporter fieldSupporter)
			: base(CodeText, FixedTextDescription, fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.TextDropEdit; }
		}
		public override int DetailMaxLength
		{
			get { return FieldSupporter.MaxLength; }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public override IList GetBoundCollection(BusinessObjectFactory factory)
		{
			return FieldSupporter.List;
		}
		public override IZType GetDefaultValue(string detail)
		{
			return new ZString(detail);
		}
	}
}
