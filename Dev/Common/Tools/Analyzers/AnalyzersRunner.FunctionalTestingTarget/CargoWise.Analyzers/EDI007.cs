using CargoWise.EntityFramework;
using CargoWise.Types;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI007
	{
		internal class RefEntity
		{
			[TranslatableDataField("RefEntity", "RC_Description")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1192:CustomizableDataCaption Asmid Required", Justification = "Asmid not applicable to test project")]
			public ZString RC_Description { get; set; }
		}

		internal void Method(RefEntity entity)
		{
			//EDI007:Customizable Data Translation Rule
			_ = entity.RC_Description;
		}
	}
}
