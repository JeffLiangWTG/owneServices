using System;

namespace Enterprise.eHubMessaging.Business
{
	class AndExpr : Query
	{
		readonly BooleanFunctions opnd1;
		readonly BooleanFunctions opnd2;

		internal AndExpr(Query opnd1, Query opnd2)
		{
			this.opnd1 = new BooleanFunctions(opnd1);
			this.opnd2 = new BooleanFunctions(opnd2);
		}

		internal override object GetValue(XPathReader reader)
		{
			object ret = this.opnd1.GetValue(reader);

			if (Convert.ToBoolean(ret))
			{
				ret = this.opnd2.GetValue(reader);
			}
			return ret;
		}

		internal override XPathResultType ReturnType()
		{
			return XPathResultType.Boolean;
		}
	}
}
