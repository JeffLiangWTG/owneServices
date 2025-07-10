using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ExportDeclarationMessageSendingObjectParent : DeclarationMessageSendingObjectParent<ExportDeclarationMessageSendingObject>
{
	public ExportDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header) => new ExportDeclarationMessageSendingObject(this, (CusEntryHeader)header);

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

	readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
	{
			new MessageSendingObjectProperty(nameof(ExportDeclarationMessageSendingObject.DeclarationNumber), true, 130),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.MessageType, true, 60),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.VOCReason, true, 160),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.DeclarationType, true, 100),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.SubStyle, true, 100),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.Description, false, 200),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.LocalReferenceNumber, true, 180),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.EntryStatus, true, 100),
	};

	protected override ZString CheckMessageSendingEnvironment()
	{
		var result = EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera();
		if (result.IsEmpty && ParentDeclaration.IsExportDeclarationActivation)
		{
			if (ParentDeclaration.JE_MessageSubType.IsEmpty)
			{
				return Res.GetString("CBDE6970-24C6-4344-83C2-9C43CB119C1A", "Activation Type is empty. Please choose an Activation Type and try again.");
			}

			if (ParentDeclaration.JE_MessageSubType == ActivationTypeList.Codes.Edec)
			{
				return EnvironmentHelper.CheckMessageSendingEnvironmentForEdec();
			}
		}
		return result;
	}

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObjectParent|IdentificationNumber", Caption = "Identification Number")]
	public ZString IdentificationNumber => ParentDeclaration.DeclarantAddress?.Header?.GetBIDNumber() ?? ZString.Empty;

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObjectParent|ContactName", Caption = "Contact Name")]
	public ZString ContactName => ParentDeclaration.CusAgent?.GS_FullName ?? string.Empty;

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObjectParent|Phone Number", Caption = "Phone Number")]
	public ZString PhoneNumber => ParentDeclaration.CusAgent?.GS_WorkPhone_Formatted ?? string.Empty;

	[ResourceStringData("CH.Business.ExportDeclarationMessageSendingObjectParent|EmailAddress", Caption = "Email")]
	public ZString EmailAddress => ParentDeclaration.CusAgent?.GS_EmailAddress ?? string.Empty;

	public MessageSendingDeclaration SendingDeclaration => sendingDeclaration ??= CreateSendingDeclaration();
	MessageSendingDeclaration sendingDeclaration;

	public bool IsExportAndAnyNC123 => Factory.GetCached(ref isExportAndAnyNC123, () => ParentDeclaration.IsExport && SelectedSendingObjects.Cast<ExportDeclarationMessageSendingObject>().Any(x => x.IsNC123));
	CachedProperty<bool> isExportAndAnyNC123;

	MessageSendingDeclaration CreateSendingDeclaration()
	{
		var sendingDeclaration = new MessageSendingDeclaration(this);
		RegisterEditableChildObject(sendingDeclaration);
		return sendingDeclaration;
	}
}
