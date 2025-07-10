using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public class CusSeal : Common.CusSeal
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		CusContainer parent;
		public CusContainer Parent
		{
			get
			{
				if (parent == null && !BK_ParentTableCode.IsEmpty && !BK_ParentID.IsEmpty)
				{
					parent = Factory.Load<CusContainer>(BK_ParentID);
				}
				return parent;
			}
		}

		protected override ShortSequenceNumberGenerator GetSequenceNumberGeneratorCore()
		{
			return Parent?.SealsSequenceNumberGenerator;
		}
	}
}
