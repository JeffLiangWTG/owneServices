using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.XmlMapping
{
	[Immutable]
	[ImmutableObject(true)]
	public class ApportionmentMethodXmlMapping : EnterpriseCodeExternalCodeMappings
	{
		protected ApportionmentMethodXmlMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(AllocationMethod.ChargeableUnits, nameof(Xsd.TxnLineConsolApportionmentMethod.CHG));
			yield return new Mapping(AllocationMethod.ContainerCount, nameof(Xsd.TxnLineConsolApportionmentMethod.CNT));
			yield return new Mapping(AllocationMethod.GrossWeight, nameof(Xsd.TxnLineConsolApportionmentMethod.GWT));
			yield return new Mapping(AllocationMethod.GrossVolume, nameof(Xsd.TxnLineConsolApportionmentMethod.GVT));
			yield return new Mapping(AllocationMethod.Revenue, nameof(Xsd.TxnLineConsolApportionmentMethod.REV));
			yield return new Mapping(AllocationMethod.Shipment, nameof(Xsd.TxnLineConsolApportionmentMethod.SHP));
			yield return new Mapping(AllocationMethod.TwentyFootEquivalentUnit, nameof(Xsd.TxnLineConsolApportionmentMethod.TEU));
			yield return new Mapping(AllocationMethod.OuterPackTotal, nameof(Xsd.TxnLineConsolApportionmentMethod.OPT));
			yield return new Mapping(AllocationMethod.CapacityPerContainer, nameof(Xsd.TxnLineConsolApportionmentMethod.CAP));
			yield return new Mapping(AllocationMethod.FreeSpaceContribution, nameof(Xsd.TxnLineConsolApportionmentMethod.FSC));
		}

		public new Xsd.TxnLineConsolApportionmentMethod GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.TxnLineConsolApportionmentMethod.CHG, errorContext, notifications);
		}

		static readonly Lazy<ApportionmentMethodXmlMapping> LazyInstance = new Lazy<ApportionmentMethodXmlMapping>(() => OverridableNewDelegate.Value != null ? OverridableNewDelegate.Value() : new ApportionmentMethodXmlMapping());

		public static ApportionmentMethodXmlMapping Instance
		{
			get { return LazyInstance.Value; }
		}

		protected delegate ApportionmentMethodXmlMapping NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected override string Name
		{
			get { return Res.GetString("7f1d7d0c-2bcd-40d7-9931-0d7af63ff712", "Apportionment Method"); }
		}
	}
}
