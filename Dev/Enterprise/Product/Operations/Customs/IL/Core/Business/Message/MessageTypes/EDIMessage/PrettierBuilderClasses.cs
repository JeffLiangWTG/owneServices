using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;

namespace Enterprise.Customs.IL.Business
{
	public class Builder
	{
		readonly ILEDIMessagePrettierBase prettier;
		readonly ZStringBuilder sb;

		public Builder(ILEDIMessagePrettierBase prettier)
			: this(prettier, new ZStringBuilder())
		{
		}

		internal Builder(ILEDIMessagePrettierBase prettier, ZStringBuilder sb)
		{
			this.prettier = prettier;
			this.sb = sb;
		}

		public Builder OfMultiple<TPayload>(Func<IEnumerable<TPayload>> payloadSelector, Action<PayloadBuilder<TPayload>> builderAction)
		{
			foreach (var payload in payloadSelector())
			{
				BuildPayload(builderAction, payload);
			}

			return this;
		}

		public Builder OfSingle<TPayload>(Func<TPayload> payloadSelector, Action<PayloadBuilder<TPayload>> builderAction)
		{
			BuildPayload(builderAction, payloadSelector());

			return this;
		}

		void BuildPayload<TPayload>(Action<PayloadBuilder<TPayload>> builderAction, TPayload payload)
		{
			if (payload != null)
			{
				var payloadBuilder = new PayloadBuilder<TPayload>(payload, sb, prettier);
				builderAction.Invoke(payloadBuilder);
			}
		}

		public ZString Build() => sb.ToString();
	}

	public class PayloadBuilder<TPayload>
	{
		internal PayloadBuilder(TPayload payload, ZStringBuilder sb, ILEDIMessagePrettierBase prettier)
		{
			this.payload = payload;
			this.sb = sb;
			this.prettier = prettier;
		}

		public PayloadBuilder<TPayload> WithResponseSection(Action<TPayload, ZStringBuilder> sectionRenderer)
		{
			RenderSection(PrettiedCaptions.Section.ResponseSection, sectionRenderer);
			return this;
		}

		public PayloadBuilder<TPayload> WithRequestSection(Action<TPayload, ZStringBuilder> sectionRenderer)
		{
			RenderSection(PrettiedCaptions.Section.RequestSection, sectionRenderer);
			return this;
		}

		public PayloadBuilder<TPayload> WithExceptionsSection<TException>(
			Func<TPayload, bool> shouldRender,
			Func<TPayload, IEnumerable<TException>> exceptionSelector,
			Func<TException, int> exceptionLevelSelector,
			Func<TException, int> exceptionTypeSelector,
			Func<TException, string> exceptionDescriptionSelector,
			Func<TException, IEnumerable<string>> exceptionParamsSelector = null,
			Dictionary<string, Func<TException, string>> additionalColumnsSelectors = null,
			string[] columnsOrder = null)
		{
			if (shouldRender(payload))
			{
				var cols = new List<TableColumn<TException>>()
					{
						new TableColumn<TException>(
							PrettiedCaptions.Exceptions.ExceptionLevel,
							ex => $"{exceptionLevelSelector(ex)} - {GetExceptionLevel(exceptionLevelSelector(ex))}"
						),
						new TableColumn<TException>(
							PrettiedCaptions.Exceptions.ExceptionType,
							ex => exceptionTypeSelector(ex).ToString()
						),
						new TableColumn<TException>(
							PrettiedCaptions.Exceptions.ExceptionDescription,
							exceptionDescriptionSelector
						)
					};

				if (exceptionParamsSelector != null)
				{
					cols.Add(new TableColumn<TException>(
						PrettiedCaptions.Exceptions.ExceptionParams,
						ex => exceptionParamsSelector(ex) != null ? string.Join("<br />", exceptionParamsSelector(ex).Where(s => !string.IsNullOrEmpty(s))) : string.Empty
						));
				}

				if (additionalColumnsSelectors != null)
				{
					foreach (var additionalColumn in additionalColumnsSelectors)
					{
						cols.Add(new TableColumn<TException>(
							additionalColumn.Key,
							additionalColumn.Value
						));
					}
				}

				if (columnsOrder != null)
				{
					cols = cols.OrderBy(c => Array.IndexOf(columnsOrder, c.Header)).ToList();
				}

				RenderSection(PrettiedCaptions.Exceptions.ExceptionsSection, (p, sb) =>
				{
					sb.Append(prettier.ToTable(exceptionSelector(payload), cols.ToArray()));
				});

				sb.Append("<br>");
			}

			return this;
		}

		public PayloadBuilder<TPayload> WithSection(Action<TPayload, ZStringBuilder> sectionRenderer)
		{
			RenderSection(ZString.Empty, sectionRenderer);
			sb.Append("<br>");

			return this;
		}

		public PayloadBuilder<TPayload> WithSectionOf<TSubPayload>(Func<TPayload, bool> shouldRender, ZString header, Func<TPayload, Func<IEnumerable<TSubPayload>>> subPayloadSelector, Action<PayloadBuilder<TSubPayload>> builderAction)
		{
			if (shouldRender(payload))
			{
				if (!header.IsEmpty)
				{
					sb.Append(prettier.ToH1IfNotEmpty(header));
				}
				var builder = new Builder(prettier, sb);
				builder.OfMultiple(subPayloadSelector.Invoke(payload), builderAction);
			}

			return this;
		}

		void RenderSection(ZString header, Action<TPayload, ZStringBuilder> sectionRenderer)
		{
			if (!header.IsEmpty)
			{
				sb.Append(prettier.ToH1IfNotEmpty(header));
			}

			sectionRenderer(payload, sb);
		}

		static string GetExceptionLevel(int exceptionLevel)
			=> exceptionLevel switch
			{
				1 => PrettiedCaptions.Exceptions.ExceptionLevelError,
				2 => PrettiedCaptions.Exceptions.ExceptionLevelWarning,
				3 => PrettiedCaptions.Exceptions.ExceptionLevelInfo,
				_ => string.Empty
			};

		readonly TPayload payload;
		readonly ZStringBuilder sb;
		readonly ILEDIMessagePrettierBase prettier;
	}

	public class TableColumn<T>
	{
		public string Header { get; }
		public string GetValue(T item) => _valueSelector(item);

		readonly Func<T, string> _valueSelector;

		public TableColumn(string header, Func<T, string> valueSelector)
		{
			Header = header;
			_valueSelector = valueSelector;
		}
	}
}
