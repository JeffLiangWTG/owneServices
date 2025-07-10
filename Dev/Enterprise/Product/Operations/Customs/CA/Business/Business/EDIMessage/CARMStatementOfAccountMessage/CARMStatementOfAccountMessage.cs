using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CARMStatementOfAccountMessage : EDIMessage, IControllerIDProvider
	{
		public CARMStatementOfAccountMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.CARMStatementOfAccount;
		}

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Messaging.EDIMessage; }
		}

		#endregion
	}
}
