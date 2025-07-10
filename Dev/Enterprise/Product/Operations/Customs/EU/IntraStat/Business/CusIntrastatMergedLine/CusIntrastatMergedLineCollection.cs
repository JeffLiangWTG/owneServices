using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public interface ICusIntrastatMergedLineCollection<out T> : IActiveBusinessObjectCollection<T>
		where T : CusIntrastatMergedLine
	{
		new T this[int index] { get; }
		void MarkAsNeedingValidation();
	}

	public class CusIntrastatMergedLineCollection<T> : ActiveBusinessObjectCollection<T>
		, ICusIntrastatMergedLineCollection<T>
		where T : CusIntrastatMergedLine
	{
		public CusIntrastatMergedLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CusIntrastatMergedLineCollection(CusIntrastatGroup master)
			: base(master.Factory, master, new ZQuery(), CusIntrastatMergedLineSchema.CIM_CIG_Group)
		{
		}
	}
}
