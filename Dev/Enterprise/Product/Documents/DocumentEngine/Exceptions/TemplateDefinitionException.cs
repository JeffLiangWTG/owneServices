using System;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class TemplateDefinitionException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		public TemplateDefinitionException(string message, CellReference cellReference)
			: base(message)
		{
			this.CellReference = cellReference;
		}

#if NETFRAMEWORK
		protected TemplateDefinitionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public override string ToString()
		{
			return String.Format((NoResString)"Error on sheet '{0}' at cell {1}: {2}", CellReference.SheetName, CellReference.Cell, Message);
		}

		#region Constructor For IJsonSerializable

		internal TemplateDefinitionException(TemplateDefinitionExceptionJsonData data)
			: base(data.Message)
		{
			this.CellReference = data.CellReference;
		}

		#endregion

		public object GetJsonData() => new TemplateDefinitionExceptionJsonData()
		{
			Message = base.Message,
			CellReference = this.CellReference
		};

		internal readonly CellReference CellReference;
	}
}
