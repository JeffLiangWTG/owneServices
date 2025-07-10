using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Manifest.Business;

public class SupportingDocSendingObject : Customs.Business.SupportingDocSendingObject
{
	public SupportingDocSendingObject(AsycudaManifestHeader supportingDocObject) : base(supportingDocObject)
	{
		Header = Argument.NotNull(supportingDocObject, nameof(supportingDocObject));
		ShouldSend = true;
	}
	public AsycudaManifestHeader Header { get; }

	protected override bool DocumentType_ReadOnly => true;

	protected override void DefaultDocType()
	{
		DocumentType = Path.GetExtension(Document?.FileName);
	}

	public static SupportingDocSendingObject New(ISupportingDocObject manifest)
	{
		var result = (SupportingDocSendingObject)manifest.GetSupportingDocSendingObject();
		return result;
	}

	[List(nameof(CUSRESList))]
	[ResourceStringData("AESupportingDocSendingObject|CUSRES", Caption = "CUSRES")]
	public ZString CUSRES
	{
		get => cUSRES;
		set
		{
			var oldValue = cUSRES;
			if (value != oldValue)
			{
				SetNonPersistentPropertyValue(CUSRESInfo, ref cUSRES, value);
				if (cUSRESReferenceDictionary.ContainsKey(value))
				{
					CUSCAR = cUSRESReferenceDictionary[value].ReferenceIdentifier;
					cUSCARMessage = null;
				}
				if (!IsValidationSuspended)
				{
					(Validation as SupportingDocSendingObjectValidation)?.ValidateCUSRES();
				}
			}
		}
	}
	ZString cUSRES;

	public ZPropertyInfo CUSRESInfo => GetZPropertyInfo(nameof(CUSRES));

	[ReadOnly(true)]
	[ResourceStringData("AESupportingDocSendingObject|CUSCAR", Caption = "CUSCAR")]
	public ZString CUSCAR
	{
		get => cUSCAR;
		set
		{
			SetNonPersistentPropertyValue(CUSCARInfo, ref cUSCAR, value);
			if (!IsValidationSuspended)
			{
				(Validation as SupportingDocSendingObjectValidation)?.ValidateCUSCAR();
			}
		}
	}
	ZString cUSCAR;

	public ZPropertyInfo CUSCARInfo => GetZPropertyInfo(nameof(CUSCAR));

	public EDIMessage CUSRESMessage => cUSRESMessageDictionary[CUSRES];

	public EDIMessage CUSCARMessage
	{
		get
		{
			if (cUSCARMessage != null)
			{
				return cUSCARMessage;
			}

			foreach (var item in CUSCARMessages)
			{
				if (DocMessageDataProvider.CUSCARMessageMatcher(item.EM_MessageText, cUSRESReferenceDictionary[CUSRES]))
				{
					cUSCARMessage = item;
					return cUSCARMessage;
				}
			}
			return default;
		}
	}
	EDIMessage cUSCARMessage;

	IEnumerable<EDIMessage> CUSRESMessages => cUSRESMessages ??= GetCUSRESMessages();
	IEnumerable<EDIMessage> cUSRESMessages;

	IEnumerable<EDIMessage> GetCUSRESMessages()
	{
		return Header.Bills.Select(b => b.Messages).SelectMany(m => m.Where(i => i.EM_ApplicationCode == ApplicationCodeList.Codes.UAECustoms && i.EM_MessageType == AEConstants.Messaging.MessageTypes.CUSRES)).ToList();
	}

	IEnumerable<EDIMessage> CUSCARMessages => cUSCARMessages ??= GetCUSCARMessages();
	IEnumerable<EDIMessage> cUSCARMessages;

	IEnumerable<EDIMessage> GetCUSCARMessages()
	{
		return Header.Bills.Select(b => b.Messages).SelectMany(m => m.Where(i => i.EM_ApplicationCode == ApplicationCodeList.Codes.UAECustoms && i.EM_MessageType == AEConstants.Messaging.MessageTypes.CUSCAR)).OrderByDescending(m => m.EM_SystemCreateTimeUtc).ToList();
	}

	public CodeDescriptionPairList CUSRESList
	{
		get
		{
			if (cacheCusresList != null)
			{
				return cacheCusresList;
			}

			cacheCusresList = new CodeDescriptionPairList();
			foreach (var item in CUSRESMessages)
			{
				var interchange = item.Interchange;
				var message = interchange.EI_BodyText;
				if (!DocMessageDataProvider.IsRFICUSRESMessage(message))
				{
					continue;
				}

				if (DocMessageDataProvider.GetInterchangeControl(message, out var controlReference, out var reference))
				{
					cacheCusresList.AddPair(controlReference, reference.ReferenceIdentifier);
					cUSRESReferenceDictionary.Add(controlReference, reference);
					cUSRESMessageDictionary.Add(controlReference, item);
				}
			}
			return cacheCusresList;
		}
	}
	CodeDescriptionPairList cacheCusresList;

	readonly Dictionary<string, EDIMessage> cUSRESMessageDictionary = new Dictionary<string, EDIMessage>();
	readonly Dictionary<string, ReferenceElements> cUSRESReferenceDictionary = new Dictionary<string, ReferenceElements>();

	public DocMessageDataProvider DocMessageDataProvider => docMessageDataProvider ??= new DocMessageDataProvider(this);
	DocMessageDataProvider docMessageDataProvider;

	protected override Customs.Business.SupportingDocSendingObjectValidation GetNewValidation() => new SupportingDocSendingObjectValidation(this);
}
