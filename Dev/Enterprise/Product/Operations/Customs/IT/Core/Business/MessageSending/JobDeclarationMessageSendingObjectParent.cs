using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using MessageSendingObjectProperty = Enterprise.Customs.Business.MessageSendingObjectProperty;

namespace Enterprise.Customs.IT.Business;

public class JobDeclarationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>, IDisposable
{
	public JobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
	{
	}

	public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;
	bool disposed;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties { get; } = new MessageSendingObjectProperty[]
	{
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.CombinedCustomsMessageSubType, true, 70),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.DeclarationType, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.DeclarationDescription, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryStatus, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageStatus, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.BGMReference, true, 150),
	};

	public ZString CustomsMessageSendingMode
	{
		get => SendingObjects?.FirstOrDefault()?.CustomsMessageSendingMode ?? ZString.Empty;
		set
		{
			foreach (var item in SendingObjects)
			{
				item.CustomsMessageSendingMode = value;
			}
		}
	}

	#region Implementation

	IEnumerable<JobDeclarationMessageSendingObject> SendingObjects => SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>();

	protected override NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectsCollection = new JobDeclarationMessageSendingObjectCollection(Factory);
		foreach (var entryHeader in ParentDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>())
		{
			var sendingObjectFactory = GetNewSendingObjectFactory(entryHeader, this);
			var sendingObject = sendingObjectFactory.TryGetNewMessageSendingObject();
			if (sendingObject != null)
			{
				sendingObjectsCollection.Add(sendingObject);
			}
		}
		RegisterEditableChildObject(sendingObjectsCollection);
		HookSelectedSendingObjectsChangeEvents();
		return sendingObjectsCollection;
	}

	protected virtual JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent)
		=> new JobDeclarationMessageSendingObjectFactory(entryHeader, sendingObjectParent);

	void HookSelectedSendingObjectsChangeEvents()
	{
		SelectedSendingObjectsChanged += OnSelectedSendingObjectsChanged;
	}

	void UnhookSelectedSendingObjectsChangeEvents()
	{
		SelectedSendingObjectsChanged -= OnSelectedSendingObjectsChanged;
	}

	void OnSelectedSendingObjectsChanged(object sender, EventArgs e)
	{
		if (sender is JobDeclarationMessageSendingObject obj)
		{
			ClearSendingObjectsShouldSendIfMoreThanOneSelected(obj);
		}
	}

	void ClearSendingObjectsShouldSendIfMoreThanOneSelected(JobDeclarationMessageSendingObject selectedSendingObject)
	{
		if (selectedSendingObject.ShouldSend && SendingObjects.Where(x => x.ShouldSend).Skip(1).Any())
		{
			ClearSendingObjectsShouldSendExceptCurrentlySelected(selectedSendingObject);
		}
	}

	void ClearSendingObjectsShouldSendExceptCurrentlySelected(JobDeclarationMessageSendingObject selectedSendingObject)
	{
		foreach (var eachObject in SendingObjects)
		{
			if (eachObject.ShouldSend && eachObject.PK != selectedSendingObject.PK)
			{
				eachObject.ShouldSend = false;
			}
		}
	}

	void IDisposable.Dispose()
	{
		if (!disposed)
		{
			UnhookSelectedSendingObjectsChangeEvents();
			disposed = true;
		}
	}

	#endregion
}
