using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceHeaderValidation : Customs.Business.InvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
		: base(invoiceHeader)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New(Parent);
	PlausiValidation plausiValidation;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateSpecialMentions();
	}

	public void ValidateSpecialMentions()
	{
		ValidateCalculatedProperty(Parent.SpecialMentionsInfo);
	}

	protected virtual void CheckSpecialMentions()
	{
		if (Parent.IsImport)
		{
			SpecialMentionsHelper.Validate(Parent.SpecialMentionsInfo, Parent.CountOfSpecialMentionsLines, GetCountOfSpecialMentionsLinesOnRelatedInvoiceHeaders);
		}
	}

	protected override void CheckJZ_UCR()
	{
		base.CheckJZ_UCR();

		PlausiValidation.CheckNS30130(Parent.JZ_UCRInfo, Parent);
	}

	protected override void CheckJZ_IncoTerm()
	{
		base.CheckJZ_IncoTerm();

		if (!Parent.IsExport)
		{
			NotificationHelper.TurnMessageErrorsIntoWarning(Parent.JZ_IncoTermInfo, notification => notification.Message.Equals(GetIncoTermIsRequiredMessage(Parent.JZ_IncoTermInfo)));
		}
	}

	protected override void CheckJZ_Weight()
	{
		base.CheckJZ_Weight();

		if (!Parent.JZ_Weight.IsEmpty && new ZWeight(Parent.JZ_Weight, Parent.JZ_WeightUQ).InKilogramsSafe != Parent.TotalWeightInKG)
		{
			Parent.JZ_WeightInfo.AddWarning(Res.GetString("46DF9C5B-626B-44EE-8C7A-E9724C0DFD87", @"The sum of Gross Weight {0} KG in Invoice Lines does not match with the total gross weight of the invoice.", Parent.TotalWeightInKG));
		}
	}

	protected override void CheckJZ_NetWeight()
	{
		base.CheckJZ_NetWeight();

		if (!Parent.JZ_NetWeight.IsEmpty && new ZWeight(Parent.JZ_NetWeight, Parent.JZ_NetWeightUQ).InKilogramsSafe != Parent.TotalNetWeightInKG)
		{
			Parent.JZ_NetWeightInfo.AddWarning(Res.GetString("22935BCC-F175-4B06-A06F-9E9FDC51A5EA", @"The sum of Net Weight {0} KG in Invoice Lines does not match with the total net weight of the invoice.", Parent.TotalNetWeightInKG));
		}
	}

	protected override string GetIncoTermIsRequiredMessage(ZPropertyInfo info) => Parent.IsExport ? MandatoryValidation.YouHaveNotEnteredMessage(info.Description) : base.GetIncoTermIsRequiredMessage(info);

	protected override void CheckJZ_Calc_CIFAmount_ZeroFreightInsurance()
	{
	}

	IEnumerable<int[]> GetCountOfSpecialMentionsLinesOnRelatedInvoiceHeaders()
	{
		foreach (var entryInstruction in Parent.InvoiceLines.Cast<JobComInvoiceLine>().GroupBy(x => x.JI_CEI).Select(x => x.First().EntryInstruction))
		{
			if (entryInstruction != null)
			{
				yield return entryInstruction.InvoiceLines.Select(x => x.InvoiceHeader).Distinct().Cast<JobComInvoiceHeader>().Select(x => x.CountOfSpecialMentionsLines).ToArray();
			}
		}
	}
}
