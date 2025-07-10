using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business
{
	[UniversalDataContext(DataContextType.EMCSJobDeclaration)]
	public class EMCSJobDeclaration : EU.EMCS.Business.EMCSJobDeclaration, Integration.Customs.GBEMCS.IEMCSJobDeclaration, ICusContainerTypeSupporter
	{
		public EMCSJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[MaxLength(CusEntryNumber.Schema.CE_EntryLineReferenceMaxLength)]
		public ZString SequenceNumber
		{
			get => LoadCusEntryNumber(false)?.CE_EntryLineReference ?? ZString.Empty;
			set
			{
				var oldValue = SequenceNumber;
				CheckMaximumLength(SequenceNumberInfo, value);
				LoadCusEntryNumber(true).CE_EntryLineReference = value;
				SequenceNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SequenceNumberInfo => GetZPropertyInfo(nameof(SequenceNumber));

		#region Credentials
		[ResourceStringData("GB.EMCSJobDeclaration.JE_CustomsProfile", Caption = "Credential")]
		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.Credentials))]
		[ReadOnlyMember(nameof(IsReadOnlyCredential))]
		public override ZString JE_CustomsProfile
		{
			get => base.JE_CustomsProfile;
			set => base.JE_CustomsProfile = value;
		}

		/// <summary>
		/// Credential Identifier field should not be editable if any messages exist against the declaration.
		/// If the field is empty however, we will allow entry to cater for existing declarations that have messages but no value in this field yet and to allow users data entry ability to fix resulting stop error.
		/// </summary>
		protected bool IsReadOnlyCredential => !JE_CustomsProfile.IsEmpty && Messages.Count > 0;

		public ICodeDescription Credential => EMCSCredentialCollection[JE_CustomsProfile];

		public CodeDescriptionPairList EMCSCredentialCollection
		{
			get
			{
				var list = Factory.GetCachedValue($"EMCSCredentialCollection-{JE_GC}", delegate
				{
					var listCached = new CodeDescriptionPairList();

					var passwordCollection = EMCSPasswordCollection;

					foreach (var r in passwordCollection.OfType<GlbExternalPassword_GB>())
					{
						listCached.AddPairIfNotExist($"{GBExtensions.GetEnterpriseCode()}.{r.EORI}.{r.Badge}", ZString.Empty);
					}
					return listCached;
				});
				return list;
			}
		}

		[ChildEditable]
		public GlbExternalPasswordCollection_GB EMCSPasswordCollection
		{
			get
			{
				if (gbGlbExternalPasswordCollection == null)
				{
					gbGlbExternalPasswordCollection = new GlbExternalPasswordCollection_GB(Company);
					gbGlbExternalPasswordCollection.Load();
					RegisterEditableChildObject(gbGlbExternalPasswordCollection);
				}

				return gbGlbExternalPasswordCollection;
			}
		}

		GlbExternalPasswordCollection_GB gbGlbExternalPasswordCollection;

		#endregion

		public new EMCSJobDeclarationLookups Lookups => (EMCSJobDeclarationLookups)base.Lookups;

		protected override JobDeclarationLookups GetNewLookups() => new EMCSJobDeclarationLookups(this);

		public new EMCSJobDeclarationValidation Validation => (EMCSJobDeclarationValidation)base.Validation;

		protected override JobDeclarationValidation GetNewValidation() => new EMCSJobDeclarationValidation(this);

		public new EMCSAddInfoJobDeclaration AddInfo => (EMCSAddInfoJobDeclaration)base.AddInfo;

		public override EU.EMCS.Business.EMCSAddInfoJobDeclaration GetNewAddInfo() => new EMCSAddInfoJobDeclaration(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (EMCSCredentialCollection.Count == 1)
			{
				ZString cred = EMCSCredentialCollection[0].Code;
				JE_CustomsProfile = cred.SubstringSafe(0, Schema.JE_CustomsProfileMaxLength);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && onSavedActions != null)
			{
				for (var i = 0; i < onSavedActions.Count; i++)
				{
					onSavedActions[i](this);
				}
			}
		}

		public void OnSuccessfulSaveDo(Action<EMCSJobDeclaration> action)
		{
			if (onSavedActions == null)
			{
				onSavedActions = new List<Action<EMCSJobDeclaration>>();
			}
			onSavedActions.Add(action);
		}

		protected override IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get
			{
				return new ZString[]
				{
					ApplicationCodeList.Codes.GbCustomsEMCS,
					ApplicationCodeList.Codes.UniversalDataMessaging
				};
			}
		}

		List<Action<EMCSJobDeclaration>> onSavedActions;
	}
}
