using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF409;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class RF409Provider
	{
		public RF409Provider(Rf409Type xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Rf409Type xmlObject;

		public ZString ApplicationReferenceId => Header?.ApplicationReferenceId;

		public ZString ApplicationDecisionCodeType => Header?.ApplicationDecisionCodeType11;

		public ZBool RefundApplicationAccepted => Extensions.IsTrueOrFalse(Header?.RefundApplicationAccepted);

		public ZString DecisionTakingCustomsAuthority => Header?.DecisionTakingCustomsAuthority;

		public ZString MRN => xmlObject.Mrn;

		public ZString TimeLimit => xmlObject.TimeLimit;

		public ZString StatementOfTheDecision => xmlObject.StatementOfTheDecision;

		public ZString DescriptionOfGrounds => xmlObject.DescriptionOfGrounds;

		public IReadOnlyCollection<GeneralRemarkProvider> GeneralRemarks => generalRemarks ??= GetGeneralRemarks();
		IReadOnlyCollection<GeneralRemarkProvider> generalRemarks;

		HeaderType Header => xmlObject.Header;

		IReadOnlyCollection<GeneralRemarkProvider> GetGeneralRemarks() => xmlObject.GeneralRemarks.Select(e => new GeneralRemarkProvider(e.GeneralRemarks)).ToArray() ?? Array.Empty<GeneralRemarkProvider>();
	}
}
