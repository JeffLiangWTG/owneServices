using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedGeographyFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionGeographyFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedGeographyFieldDefaultingStrategy(OperationalActionGeographyFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("c0db524c-1899-48fa-acdf-176ae2a7c85e", "Fixed Geography"), fieldSupporter)
		{ }

		public override FieldType DetailFieldType
		{
			get
			{
				return FieldType.Geography;
			}
		}
		public override int DetailMaxLength
		{
			get { return 127; }
		}

		public override IZType GetDefaultValue(string detail)
		{
			try
			{
				return new ZGeography(detail);
			}
			catch (FormatException)
			{
				return ZGeography.Invalid;
			}
			catch (ZTypeValueException)
			{
				return ZGeography.Invalid;
			}
		}
	}
}
