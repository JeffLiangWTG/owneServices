using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsDirectionCollection
		: DependentBusinessObjectCollection<QuarantineColsDirection, QuarantineColsHeader>
	{
		public QuarantineColsDirectionCollection(QuarantineColsHeader master) : base(master)
		{
		}

		protected override string FkColumnName => QuarantineColsDirectionSchema.Constants.QCD_QCH_ColsHeader;
	}
}
