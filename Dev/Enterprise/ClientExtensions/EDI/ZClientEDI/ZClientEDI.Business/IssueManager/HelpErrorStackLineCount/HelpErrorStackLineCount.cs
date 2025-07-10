using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorStackLineCount : AutoHelpErrorStackLineCount, IStackLine
	{
		public HelpErrorStackLineCount(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string Assembly
		{
			get { return HSL_Assembly; }
			set { HSL_Assembly = value; }
		}

		public string Type
		{
			get { return HSL_Type; }
			set { HSL_Type = value; }
		}

		public string Method
		{
			get { return HSL_Method; }
			set { HSL_Method = value; }
		}

		public string FullStackLine => HSL_StackLine;
	}
}
