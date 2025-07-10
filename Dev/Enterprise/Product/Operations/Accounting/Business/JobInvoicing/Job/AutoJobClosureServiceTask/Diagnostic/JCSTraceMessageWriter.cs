using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.MasterFiles.GUI
{
	internal class JCSTraceMessageWriter : IMessageWriter
	{
		internal JCSTraceMessageWriter(ZPropertyInfo<ZString> logProprtyInfo)
		{
			this.logProprtyInfo = logProprtyInfo;
			this.builder = new ZStringBuilder();
		}
		readonly ZPropertyInfo<ZString> logProprtyInfo;
		readonly ZStringBuilder builder;

		void IMessageWriter.WriteMessage(string message)
		{
			builder.Append(message);
			logProprtyInfo.Value = builder.ToString();
			logProprtyInfo.RefreshBinding();
		}
	}
}
