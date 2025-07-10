using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class UnderbondParent : DummyBusinessObject, ICusUnderbondDependentCollectionParent
	{
		public UnderbondParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IUnderbond Members

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get { return ""; }
		}

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		ZString IOutturnableLine.UnderbondHumanReadableName
		{
			get { return "UnderbondParent"; }
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get
			{
				return System.Array.Empty<IOutturnableLine>();
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
