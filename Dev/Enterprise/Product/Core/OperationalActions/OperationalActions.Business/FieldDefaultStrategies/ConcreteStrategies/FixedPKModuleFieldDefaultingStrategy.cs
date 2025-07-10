using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedPKModuleFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionPKModuleFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedPKModuleFieldDefaultingStrategy(OperationalActionPKModuleFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("03ceb9d1-e519-4a52-a272-4265275efdb7", "Fixed Value"), fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.Guid; }
		}
		public override int DetailMaxLength
		{
			get { return 36; }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public override IList GetBoundCollection(BusinessObjectFactory factory)
		{
			return FieldSupporter.ConstructCollection(factory);
		}
		public override IZType GetDefaultValue(string detail)
		{
			try
			{
				return new ZGuid(detail);
			}
			catch (FormatException)
			{
				return ZGuid.Invalid;
			}
		}
	}
}
