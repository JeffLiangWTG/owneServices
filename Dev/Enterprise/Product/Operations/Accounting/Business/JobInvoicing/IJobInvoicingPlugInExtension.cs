using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class IJobInvoicingPlugInExtension
	{
		public static ZString GetDefaultJobDescription(this IJobInvoicingPlugIn parent)
		{
			var result = ZString.Empty;

			var config = GetMatchingConfiguration(parent);
			if (config != null)
			{
				AddExtraText(parent, config.InvoiceDescription, ref result);
			}

			return result.Left(JobHeader.Schema.JH_DescriptionMaxLength);
		}

		public static IList<IJobInvoicingPlugIn> GetAllLinkedConsols(this IJobInvoicingPlugIn relatedShipment)
		{
			var result = new List<IJobInvoicingPlugIn>();
			if (relatedShipment?.InvoicingSupporter is IGatewayJobInvoicingSupporter gatewaySupporter)
			{
				result.AddRange(gatewaySupporter.OrderedInvoiceTargets);
			}

			return result;
		}

		#region Implementation

		static JobInvoiceDescription GetMatchingConfiguration(IJobInvoicingPlugIn parent)
		{
			JobInvoiceDescription result = null;

			var configurations = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value.ToArray<JobInvoiceDescription>();
			if (configurations.Any())
			{
				string jobTypeCode = parent.InvoicingSupporter.ConsumerType.Code.ToUpper();
				string directionCode = string.Empty;
				string modeCode = string.Empty;

				if (parent.InvoicingSupporter.ConsumerType.IsTransportModeSupported)
				{
					modeCode = parent.InvoicingSupporter.TransportMode.ToUpper();
				}

				if (parent.InvoicingSupporter.ConsumerType.IsDirectionSupported)
				{
					if (parent.InvoicingSupporter.IsExport)
					{
						directionCode = Constants.FreightShipmentDirection.Code.Export;
					}
					else if (parent.InvoicingSupporter.IsImport)
					{
						directionCode = Constants.FreightShipmentDirection.Code.Import;
					}
					else if (parent.InvoicingSupporter.IsDomestic)
					{
						directionCode = Constants.FreightShipmentDirection.Code.Domestic;
					}
					else
					{
						directionCode = Constants.FreightShipmentDirection.Code.Other;
					}
				}

				result = configurations.FirstOrDefault(x => (x.JobType.ToUpper() == parent.InvoicingSupporter.ConsumerType.Code || x.JobType.ToUpper() == RevenueRecognitionLookups.JobTypeAdditionalCodes.All) &&
					(string.IsNullOrEmpty(directionCode) || x.DirectionCode.IsEmpty || x.DirectionCode.ToUpper() == directionCode || x.DirectionCode.ToUpper() == Constants.FreightShipmentDirection.Code.All) &&
					(string.IsNullOrEmpty(modeCode) || x.Mode.IsEmpty || x.Mode.ToUpper() == modeCode || x.Mode.ToUpper() == RevenueRecognitionLookups.ModeAdditionalCodes.All));
			}

			return result;
		}

		#region Macro Resolution

		static void AddExtraText(IJobInvoicingPlugIn parent, string macro, ref ZString input)
		{
			string extraText = null;

			using (parent.Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentDirection.DEP, ContactType.All))
			{
				var provider = GetNewExtraTextMacroDataProvider(parent);
				if (provider != null)
				{
					extraText = new MacroStringReplacer(new DataProviderList(provider)).ReplaceMacros(macro);
				}
			}

			if (!string.IsNullOrEmpty(extraText))
			{
				if (input.Length > 0)
				{
					input += " " + extraText;
				}
				else
				{
					input += extraText;
				}
			}
		}

		static IBODocDataProvider GetNewExtraTextMacroDataProvider(IJobInvoicingPlugIn parent)
		{
			var wrapperCreator = ObjectFactory.Get<IDocFreightWrapperCreator>();
			IBODocDataProvider result = (IBODocDataProvider)wrapperCreator.CreateFreightWrapper((BusinessObject)parent, parent.Factory) ?? parent as IBODocDataProvider;

			return result;
		}

		#endregion

		#endregion
	}
}
