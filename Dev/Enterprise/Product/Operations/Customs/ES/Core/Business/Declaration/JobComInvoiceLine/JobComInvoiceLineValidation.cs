using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration;

public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();

		CheckMaxNumberOfVehicles(Parent);
		CheckMaxNumberOfContainers(Parent);
		CheckMaxPreviousDocument(Parent);
		ValidateDangerousGoods();
	}

	void ValidateDangerousGoods()
	{
		const int maxNumberOfUNDGsInProvisionalPeriod = 1;

		var message = SingleDangerousGoodAllowedMessage;
		Parent.ClearRowNotificationsContaining(message);
		if (IsTransitionPeriod && Parent.UNDGs.Count > maxNumberOfUNDGsInProvisionalPeriod)
		{
			Parent.AddRowMessageError(message);
		}
	}

	bool IsTransitionPeriod => Parent.Declaration?.IsTransitionPeriodAES30 ?? false;

	void CheckMaxNumberOfVehicles(JobComInvoiceLine parent)
	{
		var message = MaxNumberOfVehiclesMessage;
		Parent.ClearRowNotificationsContaining(message);
		if (parent.Vehicles.Count > 99)
		{
			if (parent.CusEntryLine != null)
			{
				parent.AddRowError(message);
			}
			else
			{
				parent.AddRowMessageError(message);
			}
		}
	}

	void CheckMaxNumberOfContainers(JobComInvoiceLine parent)
	{
		var message = MaxNumberOfContainersMessage;
		Parent.ClearRowNotificationsContaining(message);
		if (parent.ContainersPivot.Count > 99)
		{
			parent.AddRowMessageError(message);
		}
	}

	void CheckMaxPreviousDocument(JobComInvoiceLine parent)
	{
		var message = MaxPreviousDocumentMessage;
		Parent.ClearRowNotificationsContaining(message);
		if (parent.PreviousDocuments.Count > 1)
		{
			parent.AddRowMessageError(message);
		}
	}

	protected override void CheckJI_CEI()
	{
		base.CheckJI_CEI();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CEIInfo);
	}

	protected override void CheckJI_Weight()
	{
		base.CheckJI_Weight();

		MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_WeightInfo);
		MandatoryValidation.MessageErrorIfIsZero(Parent.JI_WeightInfo);
	}

	protected override void CheckJI_NetWeight()
	{
		base.CheckJI_NetWeight();

		MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_NetWeightInfo);
	}

	protected override void CheckJI_CustomsQuantity()
	{
		base.CheckJI_CustomsQuantity();
		if (!Parent.JI_Weight.IsEmpty && Parent.JI_CustomsQuantity > Parent.GrossWeightInKG)
		{
			Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("604C1ED8-1442-4008-8DEA-DF564A4206CB", "Customs Quantity[38] can not be greater than GWT[35]"));
		}
	}

	protected override bool HasValidPackagePivots
	{
		get
		{
			var anyVehicle = Parent.Vehicles?.Cast<CusVehicle>().Any() ?? false;
			var result = !anyVehicle && Parent.PackagesPivot.Any();
			var entryLine = Parent.CusEntryLine;
			if (entryLine != null)
			{
				var packagingDetailsHasNoBulkWith0 = entryLine.PackagingDetails.Any(x => !PackageHelper.PackTypeIsBulk(x.Package.CW_PackType, Parent.Factory) && x.CHC_NumberOfPacks.IsEmpty);

				result &= entryLine.CL_LineNumber > 1 || !packagingDetailsHasNoBulkWith0;
			}
			else
			{
				result &= !Parent.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any((InvoiceLinePackagePivot p) => !PackageHelper.PackTypeIsBulk(p.Package.CW_PackType, Parent.Factory) && p.CHC_NumberOfPacks.IsEmpty);
			}

			return anyVehicle || result;
		}
	}

	protected override INotificationType NotificationTypeForConditionsCheck => EntryInstruction != null && (EntryInstruction.IsH2 || EntryInstruction.IsT2L || EntryInstruction.IsT2C || EntryInstruction.IsEXS) ? CargoWise.EntityFramework.NotificationType.Warning : CargoWise.EntityFramework.NotificationType.MessageError;

	protected CusEntryInstruction EntryInstruction => Parent.EntryInstruction;

	ZString SingleDangerousGoodAllowedMessage => Res.GetString("FA437E42-D2BC-4122-A277-9AB76CD3883C", "In provisional period, only one Dangerous Goods Code can be used.");

	ZString MaxNumberOfContainersMessage => Res.GetString("68B0D2F8-A768-4505-90CA-AC19F9E1FE99", "Customs will not accept a declaration with more than 99 containers per line.");

	ZString MaxNumberOfVehiclesMessage => Res.GetString("6E3DAE29-0202-4B6B-B8A6-C3896CEF219F", "The maximum number of vehicles allowed per Invoice Line is 99. Please, if more are needed create a new Invoice Line.");

	ZString MaxPreviousDocumentMessage => Res.GetString("FA323356-319C-41AC-905F-BE8166128B42", "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");

	protected ZString MaxNumberOfSupplyChainActorMessage => Res.GetString("61BC1E5C-85DC-42E1-98C9-D0389B260F99", "Customs will not accept a declaration with more than 99 Supply Chain Actor per line.");

	protected ZString MaxNumberOfAdditionalDocumentsTRA => Res.GetString("2B9F510E-33F9-4EA4-ADB5-158BEB56A478", "Customs will not accept a declaration with more than 99 TRA Additional Documents per line.");

	protected ZString OneAdditionalDocumentsTRA_ForEntryEXS => Res.GetString("FCDA5CE1-D6DB-4D2D-82F8-2155F1D1E86C", "You have not entered a transport document for this line");

	protected ZString MaxNumberOfAdditionalDocumentsINF => Res.GetString("C914F493-4C27-4B0E-9D05-88E9A692B957", "Customs will not accept a declaration with more than 99 INF Additional Documents per line.");
}
