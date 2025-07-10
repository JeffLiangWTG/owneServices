using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.MFI.DocWrappers
{
	public class DocMFIPrintStatement : DocStatement
	{
		#region Constructors and Type Overriding

		protected DocMFIPrintStatement(PrintStatement printStatement, BusinessObjectFactory factory)
			: base(printStatement, factory)
		{
		}

		public new static DocMFIPrintStatement New(PrintStatement printStatement, BusinessObjectFactory factory)
		{
			return (printStatement != null) ? new DocMFIPrintStatement(printStatement, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocStatement OverriddenNewMethod(PrintStatement printStatement, BusinessObjectFactory factory)
		{
			return DocMFIPrintStatement.New(printStatement, factory);
		}

		#endregion

		#region Wrapper Fields

		public override Image StatementLogo
		{
			get
			{
				Image fImage;

				if (MFIConstants.NZ.ClientSpecificCondition)
				{
					if (DocumentDeliveryMode == nameof(Enterprise.ZArchitecture.Core.PrintCopyType.EML) || DocumentDeliveryMode == nameof(Enterprise.ZArchitecture.Core.PrintCopyType.FAX))
					{
						fImage = MFIDataRegistry.Instance.StatementLetterhead;
					}
					else
					{
						fImage = null; // blank image
					}
				}
				else
				{
					fImage = base.StatementLogo;
				}

				return fImage;
			}
		}

		#endregion
	}
}
