<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="html" encoding="utf-8" indent="yes" />

	<xsl:template match="/">
		<xsl:apply-templates select="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']" />
	</xsl:template>

	<xsl:template name="replace">
		<xsl:param name="text" select="string(.)"/>
		<xsl:param name="oldValue"/>
		<xsl:param name="newValue"/>
		<xsl:choose>
			<xsl:when test="contains($text, $oldValue)">
				<xsl:value-of select="substring-before($text, $oldValue)"/>
				<xsl:value-of select="$newValue"/>
				<xsl:call-template name="replace">
					<xsl:with-param name="text" select="substring-after($text, $oldValue)"/>
					<xsl:with-param name="oldValue" select="$oldValue"/>
					<xsl:with-param name="newValue" select="$newValue"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$text"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']">
		<xsl:variable name="Reason_LineFeed">
			<xsl:call-template name="replace">
				<xsl:with-param name="text" select="./*[local-name()='EventParameters']/*[local-name()='Reason']" />
				<xsl:with-param name="oldValue" select="'&#xa;'"/>
				<xsl:with-param name="newValue" select="'&lt;br/&gt;'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="Reason_Tab">
			<xsl:call-template name="replace">
				<xsl:with-param name="text" select="$Reason_LineFeed" />
				<xsl:with-param name="oldValue" select="'&#x9;'"/>
				<xsl:with-param name="newValue" select="'&#160;&#160;&#160;&#160;'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="Reason_Space">
			<xsl:call-template name="replace">
				<xsl:with-param name="text" select="$Reason_Tab" />
				<xsl:with-param name="oldValue" select="'&#x20;'"/>
				<xsl:with-param name="newValue" select="'&#160;'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html&gt;</xsl:text>
		<style>
			table {
			table-layout: fixed;
			margin-top: 10px;
			width: 100%;
			border-collapse: collapse;
			}

			td {
			font-size: 30px;
			padding-left: 10px;
			padding-right: 10px;
			vertical-align: top;
			white-space: pre-wrap;
			word-wrap: break-word;
			}
		</style>
		<table border='3'>
			<tr>
				<td>EventTime</td>
				<td colspan='4'>
					<xsl:value-of select="./*[local-name()='EventTime']"/>
				</td>
			</tr>
			<tr>
				<td>EventType</td>
				<td colspan='4'>
					<xsl:value-of select="./*[local-name()='EventType']"/>
				</td>
			</tr>
			<tr>
				<td>MessageType</td>
				<td colspan='4'>
					<xsl:value-of select="./*[local-name()='EventParameters']/*[local-name()='MessageType']"/>
				</td>
			</tr>
			<tr>
				<td>Type</td>
				<td colspan='4'>
					<xsl:value-of select="./*[local-name()='EventParameters']/*[local-name()='Type']"/>
				</td>
			</tr>
			<tr>
				<td>Reason</td>
				<td colspan='4'>
					<xsl:value-of select="$Reason_Space" disable-output-escaping='yes'/>
				</td>
			</tr>
		</table>
	</xsl:template>
</xsl:stylesheet>
