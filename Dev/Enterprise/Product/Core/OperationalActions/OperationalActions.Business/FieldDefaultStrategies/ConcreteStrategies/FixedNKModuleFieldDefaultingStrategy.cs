using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedNKModuleFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionNKModuleFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedNKModuleFieldDefaultingStrategy(OperationalActionNKModuleFieldSupporter fieldSupporter)
			: base(CodeText, FixedTextDescription, fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.TextCodeFindBox; }
		}
		public override int DetailMaxLength
		{
			get { return FieldSupporter.MaxLength; }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public override IList GetBoundCollection(BusinessObjectFactory factory)
		{
			return FieldSupporter.ConstructCollection(factory);
		}
		public override IZType GetDefaultValue(string detail)
		{
			return new ZString(detail);
		}
	}
}
