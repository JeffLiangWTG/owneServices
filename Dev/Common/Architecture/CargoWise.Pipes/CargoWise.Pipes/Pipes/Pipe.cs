using System;
using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.Pipes
{
	public class Pipe<TOutput> : IPipe<TOutput>
	{
		public Pipe(PipeType type, Delegate action, params IPipeDataSource[] inputs)
		{
			key = Guid.NewGuid();
			this.type = type;
			this.inputs = inputs;
			this.action = action;
		}

		readonly Guid key;
		readonly PipeType type;
		readonly IEnumerable<IPipeDataSource> inputs;
		readonly Delegate action;

		public string DebugName { get; set; }

		#region Explicit Implementation

		Guid IPipeDataSource.Key
		{
			get { return key; }
		}

		PipeType IPipe.Type
		{
			get { return type; }
		}

		IEnumerable<IPipeDataSource> IPipe.Inputs
		{
			get { return inputs; }
		}

		object IPipe.Do(object[] pipeInputs)
		{
			return action.DynamicInvoke(pipeInputs);
		}

		TOutput IPipe<TOutput>.Do(object[] pipeInputs)
		{
			return (TOutput)action.DynamicInvoke(pipeInputs);
		}

		Type IPipeDataSource.Output
		{
			get { return typeof(TOutput); }
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug only text")]
		public override string ToString()
		{
			return DebugName ?? string.Format(CultureInfo.InvariantCulture, "{0} Pipe -> {1}", type, typeof(TOutput).Name);
		}

		#endregion
	}
}
