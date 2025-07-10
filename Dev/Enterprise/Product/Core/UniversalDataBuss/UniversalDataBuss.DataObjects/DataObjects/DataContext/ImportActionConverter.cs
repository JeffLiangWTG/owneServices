using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[CodeAlive("It will be used in Send Manual Xml form")]
	public sealed class ImportActionConverter : EnumConverter<ImportAction>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				nameof(ImportAction.Merge),
				nameof(ImportAction.LinkOnly)
			};
		}

		protected override ImportAction[] GetEnumValues()
		{
			return new ImportAction[]
			{
				ImportAction.Merge,
				ImportAction.LinkOnly
			};
		}
	}
}
