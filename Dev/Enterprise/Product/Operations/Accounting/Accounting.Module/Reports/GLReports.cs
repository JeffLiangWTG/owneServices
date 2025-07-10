using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Aggregator;
using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GLReports : ZReportModule
	{
		public GLReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GLReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GeneralLedgerReports; }
		}

		protected override Control GetNewEmbeddedControl()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery findGLAccountsWithSectionsNotDefinedQuery = new ZQuery(AccGLHeaderSchema.AG_Column, ZString.Empty);
			findGLAccountsWithSectionsNotDefinedQuery.OrderBy = AccGLHeaderSchema.Constants.AG_Column;
			AccGLHeader[] headers = factory.Load<AccGLHeader>(findGLAccountsWithSectionsNotDefinedQuery);

			if (headers.Length > 0)
			{
				Globals.Message.ShowError(GetSectionsNotDefinedMessage(headers));
			}

			new AggregateController().PerformAggregationIfRequired();
			return base.GetNewEmbeddedControl();
		}

		#region Implementation

		ZString GetSectionsNotDefinedMessage(AccGLHeader[] headers)
		{
			ZString message = Res.GetString("d28c26f1-e57c-46ff-9baa-88753cd91284", "There are a number of GL Accounts that do not have a 'Section' defined.") + "\r\n";

			message += System.Environment.NewLine + System.Environment.NewLine;
			message += Res.GetString("65fed361-b949-49ad-be06-9c751590cc2f", "All GL Accounts must be part one of these sections:") + " \r\n";
			message += Res.GetString("67de9479-2dc5-4d18-b81d-bf17e3c5714a", "- Trading Statement") + "\r\n";
			message += Res.GetString("194eaa76-f8ff-46eb-a72b-ba02e651af32", "- Overheads") + "\r\n";
			message += Res.GetString("02fd0be5-9abb-4a2a-a9f9-770ce6d3b16c", "- Profit & Loss Appropriation") + "\r\n";
			message += Res.GetString("b976deec-1ac3-483f-9f13-2b2c6f40d613", "- Owners Equity") + "\r\n";
			message += Res.GetString("09f7162a-3c18-4e71-9c4a-6cdd60df66e1", "- Assets") + "\r\n";
			message += Res.GetString("808bf297-d9e0-49ed-af38-9cf2f4ba1e3f", "- Liabilities") + "\r\n\r\n";

			message += Res.GetString("bb5340ac-b484-4eb2-b7b8-e6eda32c042f", "Please fix the setup for these GL Accounts before printing GL Reports:");
			message += System.Environment.NewLine + System.Environment.NewLine;

			int accountsPerLine = 10;

			for (int i = 0; i < 100 && i < headers.Length; i++)
			{
				AccGLHeader header = headers[i];
				message += header.AG_AccountNum + ", ";
				accountsPerLine++;

				if (accountsPerLine == 10)
				{
					message += System.Environment.NewLine;
					accountsPerLine = 0;
				}
			}

			message = message.TrimEndIncludingWhiteSpace(',');

			if (headers.Length > 100)
			{
				message += " ... ";
			}

			message += System.Environment.NewLine + System.Environment.NewLine;
			message += Res.GetString("caba0ed6-e69c-46d2-9a02-b3cfbab80aea", "NOTE: You can use the 'Define GL Sections' button under 'Maintain > Accounts > GL Accounts' to update all of your GL accounts at once.");
			return message;
		}

		#endregion
	}
}
