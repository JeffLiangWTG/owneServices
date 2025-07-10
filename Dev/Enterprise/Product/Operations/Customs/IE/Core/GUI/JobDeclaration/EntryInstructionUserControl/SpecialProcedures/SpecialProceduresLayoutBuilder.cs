using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public class SpecialProceduresLayoutBuilder : EU.GUI.SpecialProceduresLayoutBuilder<JobDeclaration>
	{
		public SpecialProceduresLayoutBuilder()
		{
			AddControlBag(IEBag);
		}

		public SpecialProceduresControlBag IEBag => SpecialProceduresControlBag.Instance;
	}
}
