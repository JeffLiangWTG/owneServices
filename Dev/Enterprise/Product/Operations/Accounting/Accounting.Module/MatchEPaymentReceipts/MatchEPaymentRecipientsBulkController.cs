using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class MatchEPaymentRecipientsBulkController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationMatchRecipientsForEPayment; }
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
			return new MatchEPaymentRecipientsBulkForm((MatchEPaymentRecipientsBulk)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.MatchEPaymentRecipientsBulk; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MatchEPaymentRecipientsBulk); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new MatchEPaymentRecipientsBulk(Factory);
		}
	}
}
