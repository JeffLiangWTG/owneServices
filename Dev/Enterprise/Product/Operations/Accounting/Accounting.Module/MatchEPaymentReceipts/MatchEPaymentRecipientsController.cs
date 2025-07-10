using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class MatchEPaymentRecipientsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgPayablesAccountDetailsEPaymentModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MatchEPaymentRecipientsForm((MatchEPaymentRecipients)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.MatchEPaymentRecipients; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MatchEPaymentRecipients); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var accountDetailsCollection = CollectionForDefaultsAndValidation?.ToArray<AccAPAccountDetails>();
			var accountDetails = accountDetailsCollection?.FirstOrDefault();
			return new MatchEPaymentRecipients(Factory, accountDetails);
		}
	}
}
