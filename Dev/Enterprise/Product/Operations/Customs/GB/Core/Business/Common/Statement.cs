using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class Statement : IStatement
	{
		public Statement(ZString code, ZString text)
		{
			this.code = code;
			this.text = text;
		}

		public Statement(AdditionalInfo additionalInfo)
		{
			if (additionalInfo == null)
			{
				throw new ArgumentNullException(nameof(additionalInfo));
			}
			this.code = additionalInfo.CSI_Code;
			this.text = additionalInfo.CSI_Description;
			this.exportFromCountry = additionalInfo.CSI_RN_NKCountryCode;
			this.exportFromEc = additionalInfo.CSI_NctsExportFromEC;
		}

		#region IStatement Members

		ZString IStatement.Statement
		{
			get { return this.code; }
		}

		ZString IStatement.StatementText
		{
			get { return this.text; }
		}

		public ZString ExportFromCountry
		{
			get { return this.exportFromCountry; }
		}

		public ZBool ExportFromEC
		{
			get { return this.exportFromEc; }
		}

		#endregion

		readonly ZString code;
		readonly ZString text;
		readonly ZString exportFromCountry;
		readonly ZBool exportFromEc;
	}
}
