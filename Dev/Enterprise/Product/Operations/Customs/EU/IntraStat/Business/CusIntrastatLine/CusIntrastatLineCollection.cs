using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public interface ICusIntrastatLineCollection<out T> : IActiveBusinessObjectCollection<T>
		where T : CusIntrastatLine
	{
		new T this[int index] { get; }
		void MarkAsNeedingValidation();
	}

	public class CusIntrastatLineCollection<T> : ActiveBusinessObjectCollection<T>
		, ICusIntrastatLineCollection<T>
		where T : CusIntrastatLine
	{
		public CusIntrastatLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CusIntrastatLineCollection(CusIntrastatHeader master)
			: base(master.Factory, master, new ZQuery(), CusIntrastatLineSchema.CIL_CIH_Header)
		{
		}

		protected override void OnAdded(T businessObject)
		{
			base.OnAdded(businessObject);

			if (Relationship.Master is CusIntrastatHeader header)
			{
				businessObject.CIL_RX_NKCurrency = header.Company.GC_RX_NKLocalCurrency;
			}
		}
	}
}
