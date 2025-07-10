using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business;

partial class AsycudaManifestHeader : IMessageSenderSupporter, IInputReferenceProvider, IErrorMessageProcessingStrategyParent, INACCSMessageImportSupporter
{
	public bool IsHCH => AMA_ManifestType == JPManifestTypeCodeList.Codes.HCH;

	public bool IsHDF => AMA_ManifestType == JPManifestTypeCodeList.Codes.HDF;

	public bool IsNVC => AMA_ManifestType == JPManifestTypeCodeList.Codes.NVC;

	public bool IsVAN => AMA_ManifestType == JPManifestTypeCodeList.Codes.VAN;

	#region IMessageSenderSupporter

	public ISendsMessagesToCustoms MessageInitiator
	{
		get => messageInitiator;
		set => messageInitiator = value;
	}
	ISendsMessagesToCustoms messageInitiator;

	public BusinessObject BusinessObjectForNotifications => this;

	#endregion

	#region IInputReferenceProvider

	ZString IInputReferenceProvider.InputReference => AMA_InputReference;

	#endregion

	#region MessageSendingInProgress

	public IDisposable SetCurrentMessageSendingContext(IMessageSendingContext context)
	{
		return new DisposableAction(() => currentMessageSendingContext = context, () => currentMessageSendingContext = null);
	}
	IMessageSendingContext currentMessageSendingContext;

	public IMessageSendingContext MessageSendingContext => currentMessageSendingContext;

	public bool IsNVC01SendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.NVC01);

	public bool IsNVC01BondedLocationAmendmentSendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.NVC01) && (MessageSendingContext?.Action.Equals(JPMessageActionList.Codes.Five) ?? false);

	bool IsMessageSendingInProgress(string procedureCode) => currentMessageSendingContext?.ProcedureCode.Equals(procedureCode) ?? false;

	#endregion

	#region IErrorMessageProcessingStrategyParent

	IErrorMessageProcessingStrategy IErrorMessageProcessingStrategyParent.ProcessingStrategy => new ErrorMessageProcessingStrategy(this);

	#endregion

	#region INACCSMessageImportSupporter

	bool INACCSMessageImportSupporter.IsValidParent(IBusiness parent)
	{
		return TableName == parent.TableName && PK == parent.Identifier;
	}

	#endregion
}
