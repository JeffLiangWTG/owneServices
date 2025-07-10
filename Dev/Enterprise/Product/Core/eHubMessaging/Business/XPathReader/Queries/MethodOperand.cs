
namespace Enterprise.eHubMessaging.Business
{
	class MethodOperand : Query
	{
		readonly Function.FunctionType funcType;

		internal MethodOperand(Query opnd, Function.FunctionType funcType)
		{
			this.funcType = funcType;
		}

		internal override object GetValue(XPathReader reader)
		{
			object ret = null;

			switch (this.funcType)
			{
				case Function.FunctionType.FuncCount:
					ret = reader.AttributeCount;
					break;

				case Function.FunctionType.FuncPosition:
					ret = base.PositionCount;
					break;

				case Function.FunctionType.FuncNameSpaceUri:
					ret = reader.NamespaceURI;
					break;

				case Function.FunctionType.FuncLocalName:
					ret = reader.LocalName;
					break;

				case Function.FunctionType.FuncName:
					ret = reader.Name;
					break;
			}

			return ret;
		}

		internal override XPathResultType ReturnType()
		{
			if (this.funcType <= Function.FunctionType.FuncCount)
			{
				return XPathResultType.Number;
			}

			return XPathResultType.String;
		}
	}

	//internal sealed class OperandQuery : Query
	//{
	//    private object variable;
	//    private XPathResultType type;

	//    internal OperandQuery(object var, XPathResultType type)
	//    {
	//        this.variable = var;
	//        this.type = type;
	//    }

	//    internal override object GetValue(XPathReader reader)
	//    {
	//        return (this.variable);
	//    }

	//    internal override XPathResultType ReturnType()
	//    {
	//        return this.type;
	//    }

	//}
}
