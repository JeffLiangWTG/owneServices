using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CarrierServiceLevelWrapper : CodeAndDescriptionWrapper
	{
		#region Constructors

		public CarrierServiceLevelWrapper(ZString code, IBusinessObjectCollection list, BusinessObjectFactory factory) : base(code, (IList)list, factory) { }
		public CarrierServiceLevelWrapper(ZString code, CodeDescriptionPairList list, BusinessObjectFactory factory) : base(code, (IList)list, factory) { }
		public CarrierServiceLevelWrapper(ZString code, ICodeDescriptionPairList list, BusinessObjectFactory factory) : base(code, (IList)list, factory) { }
		public CarrierServiceLevelWrapper(ZString code, ZString description, BusinessObjectFactory factory) : base(code, description, factory) { }
		public CarrierServiceLevelWrapper(OrgCarrierServiceLevel businessObjectToWrap, BusinessObjectFactory factory) : base(businessObjectToWrap, factory) { }

		#endregion

		#region Empty

		internal static new CarrierServiceLevelWrapper Empty
		{
			get { return new CarrierServiceLevelWrapper(CodeAndDescriptionWrapper.Empty.Code, CodeAndDescriptionWrapper.Empty.List, CodeAndDescriptionWrapper.Empty.Factory); }
		}

		#endregion

		OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return ((OrgCarrierServiceLevel)WrappedBO); }
		}

		#region Properties

		public ZString APProfileID
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_APProfileID; }
		}

		public ZString CarrierServiceCode
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_CarrierServiceCode; }
		}

		public ZString CarrierServiceLevelDescription
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual; }
		}

		public ZString ChargeCode
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_ChargeCode; }
		}

		public ZString ProductCode
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_ProductCode; }
		}

		public ZString ProofOfDelivery
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_ProofOfDelivery; }
		}

		public ZString ServicePrintDescription
		{
			get { return CarrierServiceLevel == null ? ZString.Empty : CarrierServiceLevel.PL_ServicePrintDescription; }
		}

		public ZBool IsSignatureRequired
		{
			get { return CarrierServiceLevel != null && CarrierServiceLevel.PL_IsSignatureRequired; }
		}

		#endregion
	}
}
