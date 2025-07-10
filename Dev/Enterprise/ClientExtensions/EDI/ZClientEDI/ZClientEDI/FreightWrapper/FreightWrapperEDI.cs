using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Business
{
	public abstract class FreightWrapperEDI<T> : FreightWrapper where T : BusinessObject, IJobHeaderParent, IJobNumber
	{
		public FreightWrapperEDI(T eDIBusinessObject, BusinessObjectFactory factory)
			: base(eDIBusinessObject, factory)
		{
			ediBusinessObject = eDIBusinessObject ?? factory.GetNull<T>();
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(ZString.Empty, new CodeDescriptionPairList(), null);
		}

		protected override Job GetJob()
		{
			return new Job.Loader(ediBusinessObject).Load();
		}

		protected override ZString GetJobNumber()
		{
			return ediBusinessObject.JobNumber;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("63afb995-1211-40ac-90f7-169812e00450", "Job Number");
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return ediBusinessObject.PK;
		}

		#region Implementation

		readonly T ediBusinessObject;

		#endregion
	}
}
