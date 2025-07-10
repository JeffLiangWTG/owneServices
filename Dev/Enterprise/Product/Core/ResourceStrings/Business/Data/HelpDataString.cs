using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public class HelpDataString : AutoHelpDataString
	{
		public HelpDataString()
		{ }

		public HelpDataStringLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new HelpDataStringLookups(this);
				}
				return lookups;
			}
		}

		HelpDataStringLookups lookups;

		protected override void SetDefaultValues()
		{
			using (GetValidationSuspender())
			{
				base.SetDefaultValues();
				HD_Language = ResourceStrings.Instance.CurrentLanguage;
			}
		}

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public new HelpDataString Clone()
		{
			return (HelpDataString)base.Clone();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			HelpDataString clone = new HelpDataString();
			clone.HD_Caption = this.HD_Caption;
			clone.HD_Code = this.HD_Code;
			clone.HD_ControlIndexInParent = this.HD_ControlIndexInParent;
			clone.HD_ControlPath = this.HD_ControlPath;
			clone.HD_FullDescription = this.HD_FullDescription;
			clone.HD_IsCheckedOut = this.HD_IsCheckedOut;
			clone.HD_EditReason = this.HD_EditReason;
			clone.HD_Language = this.HD_Language;
			clone.HD_MidCaption = this.HD_MidCaption;
			clone.HD_ShortCaption = this.HD_ShortCaption;
			clone.HD_ContextClassName = this.HD_ContextClassName;
			clone.HD_ContextSourceFile = this.HD_ContextSourceFile;
			clone.HD_ContextSourceFileLine = this.HD_ContextSourceFileLine;
			clone.HD_ContextSourceFileCol = this.HD_ContextSourceFileCol;
			clone.HD_IsReadOnly = this.HD_IsReadOnly;
			return clone;
		}

		#endregion

		#region Property Overrides

		[List("Lookups+Languages")]
		public override ZString HD_Language
		{
			get { return base.HD_Language; }
			set { base.HD_Language = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString HD_Caption
		{
			get { return base.HD_Caption; }
			set { base.HD_Caption = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString HD_FullDescription
		{
			get { return base.HD_FullDescription; }
			set { base.HD_FullDescription = value; }
		}

		[ReadOnly(true)]
		public override ZBool HD_IsCheckedOut
		{
			get { return base.HD_IsCheckedOut; }
			set { base.HD_IsCheckedOut = value; }
		}

		[List("Lookups+EditReasons")]
		[ResourceStringData("4d5fe727-ef23-4676-b6b8-9010bb5491c5", Caption = "Edit Reason")]
		public override ZString HD_EditReason
		{
			get { return base.HD_EditReason; }
			set { base.HD_EditReason = value; }
		}

		[ReadOnly(true)]
		public override ZBool HD_IsReadOnly
		{
			get { return base.HD_IsReadOnly; }
			set
			{
				base.HD_IsReadOnly = value;
				if (value)
				{
					ReadOnly = true;
				}
			}
		}

		#endregion

		public bool IsEmpty
		{
			get { return HD_Caption.IsEmpty && HD_ShortCaption.IsEmpty && HD_FullDescription.IsEmpty && HD_MidCaption.IsEmpty; }
		}

		#region To/FromResourceStringData

		public ResourceStringData ToResourceStringData()
		{
			return new ResourceStringData(HD_Code, HD_ShortCaption, HD_MidCaption, HD_Caption, HD_FullDescription);
		}

		public ResourceStringData ToResourceStringData(string sourceHash, string editReason)
		{
			return new ResourceStringData(HD_Code, HD_ShortCaption, HD_MidCaption, HD_Caption, HD_FullDescription, sourceHash: sourceHash, editReason: editReason);
		}

		public static HelpDataString CreateFromResourceStringData(ResourceStringData data, string language)
		{
			var result = new HelpDataString();
			using (result.GetValidationSuspender())
			{
				result.FromResourceStringData(data);
				result.HD_Language = language;
			}
			return result;
		}

		public void FromResourceStringData(ResourceStringData data)
		{
			using (GetValidationSuspender())
			{
				if (!data.Key.Equals(HD_Code, StringComparison.InvariantCultureIgnoreCase))
				{
					HD_Code = data.Key;
				}
				if (data.ShortCaption != null)
				{
					HD_ShortCaption = data.ShortCaption;
				}
				if (data.MediumCaption != null)
				{
					HD_MidCaption = data.MediumCaption;
				}
				if (data.Caption != null)
				{
					HD_Caption = data.Caption;
				}
				if (data.FullDescription != null)
				{
					HD_FullDescription = data.FullDescription;
				}
				if (data.EditReason != null)
				{
					HD_EditReason = data.EditReason;
				}
				LoadFromMetaData(data.MetaData);
			}
		}

		public void LoadFromMetaData(ResourceStringMetaData metaData)
		{
			if (metaData != null)
			{
				HD_ContextClassName = metaData.ContextClassName;
				HD_ContextSourceFile = metaData.ContextFile;
				HD_ContextSourceFileLine = metaData.ContextFileLine;
				HD_ContextSourceFileCol = metaData.ContextFileCol;
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return HD_IsCheckedOut;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("0bca1263-c7a6-48c1-a3ab-b79ebb81e801", "This resource string is not checked out to you. You cannot undo it.");
			}
		}

		#endregion

		public IDocBuilderUsageCollection DocBuilderUsages
		{
			get
			{
				if (docBuilderUsages == null)
				{
					docBuilderUsages = ObjectFactory.Get<IDocBuilderUsageFinder>().Find(HD_Code);
				}
				return docBuilderUsages;
			}
		}
		IDocBuilderUsageCollection docBuilderUsages;
	}
}
