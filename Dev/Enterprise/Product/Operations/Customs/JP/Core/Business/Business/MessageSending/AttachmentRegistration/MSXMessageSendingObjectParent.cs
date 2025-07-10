using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;

namespace Enterprise.Customs.JP.Business
{
	public class MSXMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<MSXMessageSendingObject>, IMessageVisualObjectParentProvider
	{
		public MSXMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
			Context = declaration.MessageSendingContext ?? new MessageSendingContext();
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new MSXMessageSendingObject((CusEntryHeader)header);
		}

		protected override NonPersistentBusinessObjectCollection<MSXMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new JobDeclarationMessageSendingObjectCollection<MSXMessageSendingObject>(Factory);
			ParentDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.IsCustomsDeclarationPhaseIDCOrEDC).ForEach(h => result.Add(CreateNewJobDeclarationMessageSendingObject(h)));
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			exportPath = JPRegistry.Instance.DefaultFolderForExportingMessages.Value;
		}

		#region Export Path

		[ResourceStringData("Enterprise.Customs.JP.Business.MSXMessageSendingObjectParent|ExportPath", Caption = "Export To", FullDescription = "The folder the message will be exported to. This can be set in Registry -> Customs -> Country or Region Specific -> Japan -> NACCS Messaging -> Default Folder for Exporting Messages")]
		public ZString ExportPath
		{
			get => exportPath;
			set
			{
				if (exportPath != value)
				{
					SetNonPersistentPropertyValue(ExportPathInfo, ref exportPath, value);
					Validation.ValidateExportPath();
				}
			}
		}

		ZString exportPath;

		public ZPropertyInfo ExportPathInfo => GetZPropertyInfo(nameof(ExportPath));

		public ZBool AllowExportMessage => SelectedSendingObjects.Any() && !ExportPath.IsEmpty;

		public ZBool AllowSendMessage => SelectedSendingObjects.Any() && ParentDeclaration.IsReadyForSending;

		#endregion

		#region Validation

		protected override MessageSendingValidation GetNewMessageSendingValidation() => MessageSendingValidation.New(TopLevelBusinessObject, GetNewMessageErrorCollector(), SecurityCheckpointToSendWithMessageError ?? Env.Security.CustomsDeclarationSendWithMessageErrors, false);

		public MSXMessageSendingObjectParentValidation Validation => new (this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region IMessageVisualObjectParentProvider

		public bool UseVisualData { get; set; }

		public IMessageSendingContext Context { get; }

		public MessageVisualObjectParent VisualObjectParent => visualObjectParent ??= new MessageVisualObjectParent(Factory);
		MessageVisualObjectParent visualObjectParent;

		public IEnumerable<IMessageContentProvider> GetContentProviders()
		{
			return SelectedSendingObjects
				.Cast<MSXMessageSendingObject>()
				.Select(x => new MessageContentProvider(Factory, x));
		}

		#endregion
	}
}
