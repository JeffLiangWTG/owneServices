using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementLineGroupCollection : DependentBusinessObjectCollection<CusStatementLineGroup, CusStatementHeader>
	{
		public CusStatementLineGroupCollection(CusStatementHeader master)
			: base(master)
		{
		}

		public CusStatementLineGroup FindOrCreate(ZString importerBusinessNumber)
		{
			CusStatementLineGroup result = null;

			if (!importerBusinessNumber.IsEmpty)
			{
				result = this.OfType<CusStatementLineGroup>().FirstOrDefault(x => x.B10_ImporterCustomsID == importerBusinessNumber);

				if (result == null)
				{
					result = AddNew();
					result.B10_ImporterCustomsID = importerBusinessNumber.Left(result.B10_ImporterCustomsIDInfo.MaxLength);
				}
			}

			return result;
		}
	}
}
