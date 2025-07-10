using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class MessageVisualObject : NonPersistentBusinessObject, IEditableMessageFieldsProvider
{
	public MessageVisualObject(IMessageContentProvider contentProvider)
		: base(contentProvider.Factory)
	{
		ContentProvider = contentProvider;
	}

	public IMessageContentProvider ContentProvider { get; }

	public EditableFieldBizObjectCollection Header
	{
		get
		{
			if (header == null)
			{
				header = new EditableFieldBizObjectCollection(Factory);
				RegisterEditableChildObject(header);
			}

			return header;
		}
	}
	EditableFieldBizObjectCollection header;

	public EditableFieldBizObjectCollection CurrentItems => currentItems ??= new (Factory);
	EditableFieldBizObjectCollection currentItems;

	public List<EditableFieldBizObjectCollection> AllChildItems => allitems ??= new ();
	List<EditableFieldBizObjectCollection> allitems;

	public ZInt CurrentNumber
	{
		get => currentNumber;
		set
		{
			if (currentNumber != value)
			{
				currentNumber = value;
				RefreshCurrentItems();

				CurrentNumberInfo.RefreshBinding();
			}
		}
	}
	ZInt currentNumber;

	void RefreshCurrentItems()
	{
		CurrentItems.RemoveAll();

		if (IsValidRecordNumber(CurrentNumber))
		{
			var items = AllChildItems[CurrentNumber - 1];
			if (items != null)
			{
				CurrentItems.AddRange(items);
			}
		}

		RefreshBinding();
	}

	public ZPropertyInfo CurrentNumberInfo => GetZPropertyInfo(nameof(CurrentNumber));

	public ZInt NumberOfResults => AllChildItems.Count;

	public ZPropertyInfo NumberOfResultsInfo => GetZPropertyInfo(nameof(NumberOfResults));

	public ZInt NumberOfReadOnlyResults { get; internal set; }

	ZInt MaxNumberOfResults { get; set; }

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

	public ZBool CanCreateNewItem => IsValidRecordNumber(CurrentNumber) && NumberOfResults < MaxNumberOfResults;

	public ZBool CanDeleteItem => IsValidRecordNumber(CurrentNumber) && CurrentNumber > NumberOfReadOnlyResults;

	public ZBool CanMoveNext => IsValidRecordNumber(CurrentNumber) && IsValidRecordNumber(CurrentNumber + 1);

	public ZBool CanMovePrevious => IsValidRecordNumber(CurrentNumber) && IsValidRecordNumber(CurrentNumber - 1);

	bool IsValidRecordNumber(ZInt number)
	{
		return number > 0 && number <= NumberOfResults;
	}

	#region Implement

	public void Initialize()
	{
		if (ContentProvider != null)
		{
			InitializeCore(ContentProvider.ProcedureCode, ContentProvider.GetMessageData());
		}
	}

	void InitializeCore(string procedureCode, byte[] messageData)
	{
		Clean();

		MessageHeader = null;
		LastMessageData = messageData;
		MaxNumberOfResults = 0;
		NumberOfReadOnlyResults = 0;

		if (messageData.Length > 0)
		{
			var factory = Factory;

			JPMessageUtils.PopulateInstructionIfNeeded(procedureCode);

			var visualBuilder = NACCSFactoryService.GetMessageVisualBuilder(factory);
			var buildResult = visualBuilder.BuildEditableFields(messageData);

			var headerObjs = buildResult.EditableFields
				.Where(c => c.RepeatIndex == 0)
				.Select(c => new EditableFieldBizObject(procedureCode, c));

			Header.AddRange(headerObjs);

			var itemGroups = buildResult.EditableFields
				.Where(c => c.RepeatIndex > 0)
				.GroupBy(c => c.RepeatIndex)
				.OrderBy(c => c.Key);

			MaxNumberOfResults = itemGroups.FirstOrDefault()
				?.FirstOrDefault()
				?.FieldDefinition?.MaxRepeat ?? 0;

			var comparer = new EditableMessageFieldComparer();

			foreach (var group in itemGroups)
			{
				var itemObjs = group
					.OrderBy(c => c, comparer)
					.Select(c => new EditableFieldBizObject(procedureCode, c));

				var subItems = new EditableFieldBizObjectCollection(Factory);
				subItems.AddRange(itemObjs);

				AllChildItems.Add(subItems);
				RegisterEditableChildObject(subItems);
			}

			CurrentNumber = AllChildItems.Count > 0 ? 1 : 0;
			NumberOfReadOnlyResults = AllChildItems.Count;
			MessageHeader = buildResult.Header;
		}

		Header.Cast<EditableFieldBizObject>().ForEach(editableFieldBizObject => editableFieldBizObject.Validation.ValidateAll());
		OnInitialized?.Invoke(this, EventArgs.Empty);
	}

	public void NewEmptyChildItem()
	{
		if (CanCreateNewItem)
		{
			var lastItems = AllChildItems.LastOrDefault();
			var maxSequence = lastItems?.Cast<EditableFieldBizObject>()?.Max(c => c.SourceField.Sequence) ?? 0;

			var subItems = new EditableFieldBizObjectCollection(Factory);
			subItems.AddRange(lastItems?.Select(c =>
			{
				var field = new UserDefinedMessageField(c.SourceField);
				field.Sequence = maxSequence++;
				return new EditableFieldBizObject(ContentProvider.ProcedureCode, field);
			}));

			AllChildItems.Add(subItems);
			RegisterEditableChildObject(subItems);

			CurrentNumber = AllChildItems.Count > 0 ? AllChildItems.Count : 0;
		}
	}

	public void DeleteCurrentChildItem()
	{
		if (CanDeleteItem)
		{
			var collection = AllChildItems[CurrentNumber - 1];
			UnRegisterEditableChildObject(collection);

			AllChildItems.Remove(collection);
			CurrentNumber = AllChildItems.Count > 0 ? CurrentNumber - 1 : 0;
		}
	}

	public EventHandler OnInitialized;

	public void Clean()
	{
		header?.RemoveAll();

		allitems?.ForEach(subItems =>
		{
			UnRegisterEditableChildObject(subItems);
			subItems.RemoveAll();
		});

		allitems?.Clear();

		currentItems?.RemoveAll();
		CurrentNumber = 0;
	}

	public byte[] BuildMessage()
	{
		if (MessageHeader != null && HasChangedValues())
		{
			var factory = Factory;
			var writer = NACCSFactoryService.GetOutboundMessageWriter(factory);
			return writer.Write(MessageHeader, this);
		}
		else
		{
			return LastMessageData;
		}
	}

	public bool HasChangedValues()
	{
		return Header.Concat(AllChildItems.SelectMany(c => c)).Cast<EditableFieldBizObject>().Any(c => !c.OriginalValue.Equals(c.OverrideValue));
	}

	byte[] LastMessageData { get; set; }

	IJPOutboundMessageHeader MessageHeader { get; set; }

	#endregion

	#region IMessageFieldsProvider

	IEnumerable<IEditableMessageField> IEditableMessageFieldsProvider.GetFields()
	{
		var result = new List<IEditableMessageField>();

		result.AddRange(Header.Select(c => c.SourceField));
		result.AddRange(AllChildItems.SelectMany(c => c.Select(e => e.SourceField)));

		return result;
	}

	#endregion
}
