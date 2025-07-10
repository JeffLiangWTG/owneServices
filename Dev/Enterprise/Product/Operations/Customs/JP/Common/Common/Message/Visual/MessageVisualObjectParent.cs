using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public interface IMessageVisualObjectParentProvider
{
	bool UseVisualData { get; set; }
	IMessageSendingContext Context { get; }
	MessageVisualObjectParent VisualObjectParent { get; }
	IEnumerable<IMessageContentProvider> GetContentProviders();
}

public sealed class MessageVisualObjectParent : NonPersistentBusinessObject
{
	public MessageVisualObjectParent(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	[ChildEditable(true)]
	public MessageVisualObjectCollection VisualObjects
	{
		get
		{
			if (visualObjects == null)
			{
				visualObjects = new MessageVisualObjectCollection(Factory);
				RegisterEditableChildObject(visualObjects);
			}

			return visualObjects;
		}
	}
	MessageVisualObjectCollection visualObjects;

	public void InitializeVisualObjects(IEnumerable<IMessageContentProvider> contentProviders)
	{
		currentNumber = 0;
		CurrentVisualObject = null;

		visualObjects?.Cast<MessageVisualObject>()
			?.ToArray()
			?.ForEach(visualObject =>
			{
				visualObject.Clean();
				visualObjects.RemoveAndDelete(visualObject);
			});

		contentProviders?.ForEach(contentProvider =>
		{
			VisualObjects.Add(new MessageVisualObject(contentProvider));
		});

		CurrentNumber = VisualObjects.Count > 0 ? 1 : 0;
	}

	public ZInt CurrentNumber
	{
		get => currentNumber;
		set
		{
			if (currentNumber != value)
			{
				currentNumber = value;
				InitializeVisualObjIfNeeded();

				CurrentNumberInfo.RefreshBinding();
			}
		}
	}
	ZInt currentNumber;

	void InitializeVisualObjIfNeeded()
	{
		if (IsValidRecordNumber(CurrentNumber))
		{
			CurrentVisualObject = VisualObjects[CurrentNumber - 1];

			if (CurrentVisualObject != null && CurrentVisualObject.Header.Count == 0)
			{
				CurrentVisualObject.Initialize();
			}
		}

		CurrentVisualObjectChanged?.Invoke(this, EventArgs.Empty);
		RefreshBinding();
	}

	public MessageVisualObject CurrentVisualObject { get; private set; }

	public EventHandler CurrentVisualObjectChanged;

	public ZPropertyInfo CurrentNumberInfo => GetZPropertyInfo(nameof(CurrentNumber));

	public ZInt NumberOfResults => VisualObjects.Count;

	public ZPropertyInfo NumberOfResultsInfo => GetZPropertyInfo(nameof(NumberOfResults));

	public bool MovePrevious()
	{
		if (CanMovePrevious)
		{
			CurrentNumber--;
			return true;
		}

		throw new NotSupportedException("Can't move before the first item.");
	}

	public bool MoveNext()
	{
		if (CanMoveNext)
		{
			CurrentNumber++;
			return true;
		}

		throw new NotSupportedException("Can't move next when on the last item.");
	}

	public ZBool CanMoveNext => IsValidRecordNumber(CurrentNumber) && IsValidRecordNumber(CurrentNumber + 1);

	public ZBool CanMovePrevious => IsValidRecordNumber(CurrentNumber) && IsValidRecordNumber(CurrentNumber - 1);

	bool IsValidRecordNumber(ZInt number)
	{
		return number > 0 && number <= NumberOfResults;
	}
}
