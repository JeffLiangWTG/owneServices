using System.Data;
using System.Diagnostics;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.MasterFiles.Business;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentVisualizer.Business
{
	[DebuggerDisplay("{" + nameof(SO_Name) + "}")]
	public sealed class VisualizerTemplate : StmTemplateBase
	{
		public VisualizerTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SO_TemplateType = StmTemplateTypes.Codes.Form;
		}

		[BusinessObjectTestExclude]
		public override ZBlob SO_Template
		{
			get => base.SO_Template;
			set
			{
				try
				{
					base.SO_Template = value;
				}
				catch (ExcelInterfaceException)
				{
					base.SO_Template = ZBlob.Empty;
				}

				if (SO_Template != ZBlob.Empty)
				{
					var template = GetOrCreateTemplate(true);

					SO_DataContext = template?.DataContext
						?? ZString.Empty;
				}
				else
				{
					template = null;
					SO_DataContext = ZString.Empty;
				}
			}
		}

		ITemplate GetOrCreateTemplate(bool forceCreate)
		{
			if (template == null
				|| forceCreate)
			{
				var worksheet = GetFlexCelWorksheet();

				var res = worksheet?.CreateTemplate();

				template = res.HasValue && res.Value.IsRight
					? res.Value.Right
					: null;
			}

			return template;
		}

		ITemplate template;

		protected override StmTemplateValidation GetNewValidation()
		{
			return new VisualizerTemplateValidation(this);
		}

		#endregion

		#region Implementation

		public IWorksheet GetFlexCelWorksheet()
		{
			if (SO_Template == null)
			{
				return null;
			}

			using (var templateStream = new MemoryStream(SO_Template))
			{
				try
				{
					return FlexCelWorksheet.FromStream(templateStream);
				}
				catch (FlexCelXlsAdapterException)
				{
					//eat
					return null;
				}
			}
		}

		public bool IsBillOfLadingTemplate
		{
			get
			{
				var template = GetOrCreateTemplate(false);
				return template?.Kind == TemplateKind.HouseBill;
			}
		}

		#endregion
	}
}
