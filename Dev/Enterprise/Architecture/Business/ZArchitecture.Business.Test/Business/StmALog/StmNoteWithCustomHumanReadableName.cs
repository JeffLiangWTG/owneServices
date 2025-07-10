using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class StmNoteWithCustomHumanReadableName : StmNote
	{
		public StmNoteWithCustomHumanReadableName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Paradigm Studio 100 V3"; }
		}
	}
}
