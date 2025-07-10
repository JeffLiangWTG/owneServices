using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Base
{
	public abstract partial class MultipleReversingBaseForm : AccountingZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		protected MultipleReversingBaseForm()
		{
			InitializeComponent();
		}

		protected MultipleReversingBaseForm(MultipleReversingProviderBase multipleReversingProvider)
			: base(multipleReversingProvider)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);

			MultipleReversingProvider.BizObjectsAlreadyReversed.CountChanged += new CollectionCountChangedEventHandler(BizObjectsAlreadyReversed_CountChanged);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RemoveErrorTransactionsButton.CaptionResourceString = RemoveErrorTransactionsButtonCaption;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			MultipleReversingProvider.BizObjectsAlreadyReversed.CountChanged -= new CollectionCountChangedEventHandler(BizObjectsAlreadyReversed_CountChanged);
		}

		MultipleReversingProviderBase MultipleReversingProvider
		{
			get { return (MultipleReversingProviderBase)BusinessEntity; }
		}

		protected override void MakeRequiredFieldsEditableForReversing()
		{
			MultipleReversingProvider.SetReadOnlyIncludingChildren(false);
			foreach (BusinessObject bizObject in MultipleReversingProvider.BizObjectsAlreadyReversed)
			{
				MakeRequiredFieldsEditableForReversingOnBizObject(bizObject);
			}
		}

		void MakeRequiredFieldsEditableForReversingOnBizObject(BusinessObject bizObject)
		{
			var wrappedObject = MultipleReversingProvider.GetWrappedBusinessEntity(bizObject);
			wrappedObject.SetReadOnlyIncludingChildren(true);
			if (!wrappedObject.HasRowErrors)
			{
				MakeRequiredFieldsEditableForReversing(wrappedObject);
			}
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			MakeRequiredFieldsEditable();
			if (DisplayMode != ODisplayMode.Delete)
			{
				base.SetReadOnlyIncludingChildren();
			}
		}

		protected override void HandleSaveException(Exception ex)
		{
			try
			{
				base.HandleSaveException(ex);
			}
			finally
			{
				if (ex is OnSavingCriticalCheckException)
				{
					this.Close();
				}
			}
		}

		protected void RemoveErrorTransactionsButton_Click(object sender, EventArgs e)
		{
			var nonReversibleBizObjects = new List<BusinessObject>();

			foreach (var bizo in MultipleReversingProvider.BizObjectsAlreadyReversed)
			{
				var reversingResultBizo = MultipleReversingProvider.GetWrappedBusinessEntity(bizo);
				if (!(reversingResultBizo as IReversing)?.IsReverseTransaction ?? reversingResultBizo.HasRowErrors)
				{
					nonReversibleBizObjects.Add(bizo);

					if (reversingResultBizo.HasChanges)
					{
						ErrorReporter.ReportOnce(string.Format("We will save this bizo changes when we do not expect it has any changes as it is original not reversed transaction. {0}", reversingResultBizo.GetAllPropertyValues()));
					}
				}
			}

			foreach (var bizo in nonReversibleBizObjects)
			{
				MultipleReversingProvider.BizObjectsAlreadyReversed.Remove(bizo);
			}
		}

		void BizObjectsAlreadyReversed_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				MultipleReversingProvider.SetReadOnlyIncludingChildren(false);
				MakeRequiredFieldsEditableForReversingOnBizObject(e.BizObject);
			}

			bool isThereSomthingToPost = MultipleReversingProvider.BizObjectsAlreadyReversed.Count > 0;
			PostingButtons.SaveButton.Enabled = isThereSomthingToPost;
			PostingButtons.SaveAndCloseButton.Enabled = isThereSomthingToPost;
		}

		protected virtual ResourceStringData RemoveErrorTransactionsButtonCaption => ResourceStringData.Empty;
	}
}
