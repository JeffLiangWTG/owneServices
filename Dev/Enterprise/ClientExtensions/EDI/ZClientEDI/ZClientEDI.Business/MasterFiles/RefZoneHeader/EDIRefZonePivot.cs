using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIRefZonePivot : RefZonePivot
	{
		public EDIRefZonePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override RefZonePivotValidation GetNewValidation()
		{
			return new EDIRefZonePivotValidation(this);
		}
	}
}

